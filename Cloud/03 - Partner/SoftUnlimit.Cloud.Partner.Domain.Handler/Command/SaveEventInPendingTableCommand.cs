using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Utility;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
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
                    await TypeHelper.AddFromEventAsync(partnerId, @event, _jnRewardPendingRepository, _saleforcePendingRepository, ct);

                    _logger.LogDebug("Save in {Partner} {@Event}", partnerId, @event);
                }
                else
                    _logger.LogDebug("Skip to {Partner} {@Event}", partnerId, @event);
            }
            if (updated)
                await _unitOfWork.SaveChangesAsync(ct);

            return command.OkResponse();
        }
    }
}
