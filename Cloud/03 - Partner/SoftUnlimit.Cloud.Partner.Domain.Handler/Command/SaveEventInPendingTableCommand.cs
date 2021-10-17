using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Command
{
    public class SaveEventInPendingTableCommand : CloudCommand
    {
        public SaveEventInPendingTableCommand(Guid id, IdentityInfo user, CreateGenericCloudEvent @event) : 
            base(id, user)
        {
            Event = @event;
        }

        public CreateGenericCloudEvent Event { get; set; }
    }
    public sealed class SaveEventInPendingTableCommandHandler : 
        ICloudCommandHandler<SaveEventInPendingTableCommand>
    {
        private readonly PartnerOptions _options;
        private readonly ICloudUnitOfWork _unitOfWork;
        private readonly ICloudRepository<JnRewardPending> _jnRewardPendingRepository;
        private readonly ICloudRepository<SaleforcePending> _saleforcePendingRepository;
        private readonly ILogger<SaveEventInPendingTableCommandHandler> _logger;


        public SaveEventInPendingTableCommandHandler(
            IOptions<PartnerOptions> options, 
            ICloudUnitOfWork unitOfWork, 
            ICloudRepository<JnRewardPending> jnRewardPendingRepository, 
            ICloudRepository<SaleforcePending> saleforcePendingRepository, 
            ILogger<SaveEventInPendingTableCommandHandler> logger)
        {
            _options = options.Value;
            _unitOfWork = unitOfWork;
            _jnRewardPendingRepository = jnRewardPendingRepository;
            _saleforcePendingRepository = saleforcePendingRepository;
            _logger = logger;
        }

        public async Task<ICommandResponse> HandleAsync(SaveEventInPendingTableCommand command, CancellationToken ct = default)
        {
            bool updated = false;
            var @event = command.Event;
            foreach (var (partnerId, settings) in _options)
            {
                if (settings.Events.Contains("*") || settings.Events.Contains(@event.Name))
                {
                    updated = true;
                    var task = partnerId switch
                    {
                        PartnerValues.Saleforce => _saleforcePendingRepository.AddAsync(FromEvent(@event, new SaleforcePending()), ct),
                        PartnerValues.JnReward => _jnRewardPendingRepository.AddAsync(FromEvent(@event, new JnRewardPending()), ct),
                        _ => throw new NotSupportedException($"Parner {partnerId} not suported")
                    };
                    await task;
                }
            }
            if (updated)
                await _unitOfWork.SaveChangesAsync(ct);

            return command.OkResponse();
        }

        private static T FromEvent<T>(CreateGenericCloudEvent @event, T pending) where T : Pending
        {
            pending.EventId = @event.Id;
            pending.SourceId = @event.SourceId.ToString();
            pending.Version = @event.Version;
            pending.ServiceId = @event.ServiceId;
            pending.WorkerId = @event.WorkerId;
            pending.CorrelationId = @event.CorrelationId;
            pending.Created = DateTime.UtcNow;
            pending.Name = @event.Name;
            pending.Body = JsonUtility.Serialize(@event.Body);
            pending.IdentityId = @event.IdentityId;
            pending.PartnerId = @event.PartnerId;
            pending.Retry = 0;
            pending.Scheduler = pending.Created;

            return pending;
        }
    }
}
