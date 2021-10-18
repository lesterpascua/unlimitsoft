using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Cloud.Command;
using SoftUnlimit.Cloud.Partner.Data;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Cloud.Security.Cryptography;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using SoftUnlimit.Security;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Command
{
    public sealed class SaleforceMessageArrivedCommand : CloudCommand
    {
        public SaleforceMessageArrivedCommand(Guid id, IdentityInfo user) : 
            base(id, user)
        {
        }

        /// <summary>
        /// Message ReplayId
        /// </summary>
        public long ReplayId { get; set; }
        /// <summary>
        /// Saleforce event name (channel name)
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// Saleforce message.
        /// </summary>
        public SalesforceCloudPayload Payload { get; set; }
    }
    public sealed class SaleforceMessageArrivedCommandHandler :
        ICloudCommandHandler<SaleforceMessageArrivedCommand>
    {
        private readonly IEventBus _eventBus;
        private readonly ICloudIdGenerator _gen;
        private readonly ICloudUnitOfWork _unitOfWork;
        private readonly ICloudRepository<SalesforceReplay> _replayRepository;
        private readonly ILogger<SaleforceMessageArrivedCommandHandler> _logger;

        public SaleforceMessageArrivedCommandHandler(
            IEventBus eventBus,
            ICloudIdGenerator gen,
            ICloudUnitOfWork unitOfWork,
            ICloudRepository<SalesforceReplay> replayRepository,
            ILogger<SaleforceMessageArrivedCommandHandler> logger
        )
        {
            _eventBus = eventBus;
            _gen = gen;
            _unitOfWork = unitOfWork;
            _replayRepository = replayRepository;
            _logger = logger;
        }

        public async Task<ICommandResponse> HandleAsync(SaleforceMessageArrivedCommand command, CancellationToken ct = default)
        {
            Guid? externalId;
            var message = command.Payload;
            if (Guid.TryParse(message.ExternalId, out var salesForceId) == false)
            {
                externalId = null;
                _logger.LogWarning("Event {ReplayId}, {ExternalId} is not a Guid", command.ReplayId, message.ExternalId);
            }
            else
                externalId = salesForceId;

            CreateGenericCloudEvent @event;
            if (!string.IsNullOrEmpty(message.Name))
            {
                try
                {
                    var body = JsonUtility.Deserialize<object>(message.Body);
                    if (body is not null && !string.IsNullOrEmpty(message.SfRecordId))
                        JsonUtility.AddNode(body, nameof(SalesforceCloudPayload.SfRecordId), message.SfRecordId);

                    @event = CreateEventFromName(message, externalId, command.ReplayId, body);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid body, RepayId: {ReplayId}, Body: {Body}", command.ReplayId, message.Body);
                    await UpdateRepayIdInDatabase(command, ct);

                    return command.ErrorResponse(command.ReplayId);
                }

                await AddEventToProcessQueue(@event, ct);
                await UpdateRepayIdInDatabase(command, ct);
            }

            return command.OkResponse();
        }

        public async Task AddEventToProcessQueue(CreateGenericCloudEvent @event, CancellationToken ct)
        {
            await _eventBus.PublishAsync(@event, ct);
        }
        private async Task UpdateRepayIdInDatabase(SaleforceMessageArrivedCommand command, CancellationToken ct)
        {
            var lastReplay = await _replayRepository
                .FindAll()
                .FirstOrDefaultAsync(p => p.EventName == command.EventName, ct);

            if (lastReplay is null)
            {
                lastReplay = new SalesforceReplay { EventName = command.EventName, ReplayId = command.ReplayId };
                await _replayRepository.AddAsync(lastReplay, ct);
            }
            else
                lastReplay.ReplayId = Math.Max(command.ReplayId, lastReplay.ReplayId);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        private CreateGenericCloudEvent CreateEventFromName(SalesforceCloudPayload salesforceEvent, Guid? externalId, long replayId, object body)
        {
            var id = _gen.GenerateId();
            var @event = new CreateGenericCloudEvent(
                id, externalId ?? Guid.Empty, replayId, _gen.ServiceId, _gen.WorkerId, id.ToString(), null, null, null, false, body)
            {
                Name = salesforceEvent.Name
            };
            return @event;
        }
    }
}
