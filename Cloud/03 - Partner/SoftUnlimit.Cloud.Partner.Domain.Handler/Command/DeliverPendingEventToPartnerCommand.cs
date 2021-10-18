using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Bus.Hangfire;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Utility;
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
        private readonly IEnumerable<IRoutingEvent> _routings;

        /// <summary>
        /// Amout of element
        /// </summary>
        public const int BatchSize = 10;


        public DeliverPendingEventToPartnerCommandHandler(
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
            var repository = TypeHelper.GetPendingQueryable(
                command.PartnerId, 
                _jnRewardPendingRepository, 
                _saleforcePendingRepository
            );

            bool hasErr = false;
            Pending[] pendings = null;
            do
            {
                pendings = await repository
                    .OrderBy(e => e.Id)
                    .Take(BatchSize)
                    .ToArrayAsync(ct);
                var first = pendings.FirstOrDefault();
                if (first == null || DateTime.UtcNow < first.Scheduler)
                    return command.OkResponse(false);

                var notificationType = _options.GetType(command.PartnerId);
                var routing = _routings.FirstOrDefault(p => p.Type == notificationType);
                if (routing == null)
                    return command.OkResponse(false);

                _logger.LogDebug("Events to notify: {Count}", pendings.Length);

                Pending currPending = null;
                try
                {
                    foreach (var pending in pendings)
                    {
                        currPending = pending;
                        bool success = await routing.RouteAsync(command.PartnerId, pending, ct);
                        if (!success)
                            throw new InvalidOperationException("Not able to publich the event in to the partner.");

                        await TypeHelper.MoveAsync(
                            command.PartnerId,
                            pending,
                            _saleforcePendingRepository,
                            _saleforceCompleteRepository,
                            _jnRewardPendingRepository,
                            _jnRewardCompleteRepository,
                            ct
                        );
                        _logger.LogInformation("{Parner} processed {@Pending}", command.PartnerId, pending);
                    }
                }
                catch (Exception ex)
                {
                    hasErr = true;
                    if (currPending is not null)
                    {
                        currPending.Retry += 1;
                        currPending.Scheduler = DateTime.UtcNow.AddSeconds(Math.Min(10 * currPending.Retry, 5 * 60));
                        _logger.LogError(ex, "Error trying to publish in {Partner}, {@Pending}.", currPending, command.PartnerId);
                    }
                    else
                        _logger.LogError(ex, "Error trying to publish in {Partner}", command.PartnerId);
                }

                await _unitOfWork.SaveChangesAsync(ct);
            } while (!hasErr && pendings.Length == BatchSize);

            return command.OkResponse(pendings?.Any() == true);
        }
    }
}