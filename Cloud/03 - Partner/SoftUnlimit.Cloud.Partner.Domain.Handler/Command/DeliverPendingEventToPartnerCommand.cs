using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Bus.Hangfire;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler
{
    /// <summary>
    /// Try to send message to partner.
    /// </summary>
    public sealed class DeliverPendingEventToPartnerCommand : CloudCommand, ISchedulerCommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="user"></param>
        public DeliverPendingEventToPartnerCommand(Guid id, IdentityInfo user) :
            base(id, user)
        {
        }

        /// <summary>
        /// Parner to check pending events.
        /// </summary>
        public PartnerValues PartnerId { get; set; }
        /// <summary>
        /// Command delay
        /// </summary>
        public TimeSpan? Delay { get; set; } = TimeSpan.FromSeconds(10);
    }
    /// <summary>
    /// Bussiness logic asociate to <see cref="DeliverPendingEventToPartnerCommand"/>
    /// </summary>
    public sealed class DeliverPendingEventToPartnerCommandHandler :
        ICloudCommandHandler<DeliverPendingEventToPartnerCommand>
    {
        private readonly ICloudUnitOfWork _unitOfWork;
        private readonly ICloudRepository<JnRewardPending> _jnRewardPendingRepository;
        private readonly ICloudRepository<JnRewardComplete> _jnRewardCompleteRepository;
        private readonly ICloudRepository<SaleforcePending> _saleforcePendingRepository;
        private readonly ICloudRepository<SaleforceComplete> _saleforceCompleteRepository;
        private readonly ILogger<DeliverPendingEventToPartnerCommandHandler> _logger;
        private readonly PartnerOptions _options;
        //private readonly ICommandBus _commandBus;
        private readonly IEnumerable<IRoutingEvent> _routings;

        private const int BatchSize = 10;


        public DeliverPendingEventToPartnerCommandHandler(
            //ICommandBus commandBus,
            IOptions<PartnerOptions> options,
            IEnumerable<IRoutingEvent> routings,
            ICloudUnitOfWork unitOfWork,
            ICloudRepository<JnRewardPending> jnRewardPendingRepository,
            ICloudRepository<JnRewardComplete> jnRewardCompleteRepository,
            ICloudRepository<SaleforcePending> saleforcePendingRepository,
            ICloudRepository<SaleforceComplete> saleforceCompleteRepository,
            ILogger<DeliverPendingEventToPartnerCommandHandler> logger
        )
        {
            _options = options.Value;
            //_commandBus = commandBus;
            _routings = routings;
            _unitOfWork = unitOfWork;
            _jnRewardPendingRepository = jnRewardPendingRepository;
            _jnRewardCompleteRepository = jnRewardCompleteRepository;
            _saleforcePendingRepository = saleforcePendingRepository;
            _saleforceCompleteRepository = saleforceCompleteRepository;
            _logger = logger;
        }

        public async Task<ICommandResponse> HandleAsync(DeliverPendingEventToPartnerCommand command, CancellationToken ct = default)
        {
            var repository = command.PartnerId switch
            {
                PartnerValues.Saleforce => _saleforcePendingRepository.FindAll().Cast<Pending>(),
                PartnerValues.JnReward => _jnRewardPendingRepository.FindAll().Cast<Pending>(),
                _ => throw new NotSupportedException($"Parner: {command.PartnerId}")
            };

            Pending[] events = null;
            do
            {
                events = await repository
                    .OrderBy(e => e.Id)
                    .Take(BatchSize)
                    .ToArrayAsync(ct);
                var first = events.FirstOrDefault();
                if (first == null || DateTime.UtcNow < first.Scheduler)
                    return command.OkResponse(false);

                var notificationType = _options.GetType(command.PartnerId);
                var routing = _routings.FirstOrDefault(p => p.Type == notificationType);
                if (routing == null)
                    return command.OkResponse(false);

                _logger.LogDebug("Events to notify: {Count}", events.Length);

                Pending currEvent = null;
                try
                {
                    foreach (var @event in events)
                    {
                        currEvent = @event;
                        bool success = await routing.RouteAsync(command.PartnerId, @event, ct);
                        if (!success)
                            throw new InvalidOperationException("Not able to publich the event in to the partner.");

                        switch (command.PartnerId)
                        {
                            case PartnerValues.Saleforce:
                                _saleforcePendingRepository.Remove((SaleforcePending)@event);
                                await _saleforceCompleteRepository.AddAsync(FromPending(@event, new SaleforceComplete()), ct);
                                break;
                            case PartnerValues.JnReward:
                                _jnRewardPendingRepository.Remove((JnRewardPending)@event);
                                await _jnRewardCompleteRepository.AddAsync(FromPending(@event, new JnRewardComplete()), ct);
                                break;
                        }
                        _logger.LogInformation("{Parner} processed {@Event}", command.PartnerId, @event);
                    }
                }
                catch (Exception ex)
                {
                    if (currEvent is not null)
                    {
                        currEvent.Retry += 1;
                        currEvent.Scheduler = DateTime.UtcNow.AddSeconds(Math.Min(10 * currEvent.Retry, 5 * 60));
                        _logger.LogError(ex, "Error trying to publish in {Partner}, {@Event}.", currEvent, command.PartnerId);
                    }
                    else
                        _logger.LogError(ex, "Error trying to publish in {Partner}", command.PartnerId);
                }

                await _unitOfWork.SaveChangesAsync(ct);
            } while (events?.Any() == true);
            //await _commandBus.SendAsync(command, ct);                       // reenqueue the command to check again

            return command.OkResponse(events.Any());
        }

        private static T FromPending<T>(Pending pending, T complete) where T : Complete
        {
            complete.Body = pending.Body;
            complete.Completed = DateTime.UtcNow;
            complete.CorrelationId = pending.CorrelationId;
            complete.Created = pending.Created;
            complete.EventId = pending.EventId;
            complete.IdentityId = pending.IdentityId;
            complete.Name = pending.Name;
            complete.PartnerId = pending.PartnerId;
            complete.Retry = pending.Retry;
            complete.ServiceId = pending.ServiceId;
            complete.SourceId = pending.SourceId;
            complete.Version = pending.Version;
            complete.WorkerId = pending.WorkerId;

            return complete;
        }
    }
}