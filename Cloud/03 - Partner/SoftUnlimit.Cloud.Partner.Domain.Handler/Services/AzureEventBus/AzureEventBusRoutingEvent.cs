using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Events;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.EventBus.Azure;
using SoftUnlimit.EventBus.Azure.Configuration;
using SoftUnlimit.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Services.AzureEventBus
{
    /// <summary>
    /// 
    /// </summary>
    public class AzureEventBusRoutingEvent : IRoutingEvent
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly PartnerOptions _options;
        private readonly ILogger<AzureEventBusRoutingEvent> _logger;
        private readonly ILogger<AzureEventBus<FakeAlias>> _busLogger;
        private readonly Dictionary<PartnerValues, Bucket> _cache = new();

        public AzureEventBusRoutingEvent(
            IOptions<PartnerOptions> options,
            ILogger<AzureEventBusRoutingEvent> logger,
            ILogger<AzureEventBus<FakeAlias>> busLogger)
        {
            _options = options.Value;
            _logger = logger;
            _busLogger = busLogger;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc />
        public NotificationType Type => NotificationType.AzureEventBus;
        /// <inheritdoc />
        public async Task<bool> RouteAsync(PartnerValues partnerId, Pending pending, CancellationToken ct = default)
        {
            var eventBus = await CreateOrGetAsync(partnerId, ct);

            var body = JsonUtility.Deserialize<object>(pending.Body);
            var @event = new CreateGenericCloudEvent(
                pending.EventId,
                Guid.Parse(pending.SourceId),
                pending.Version,
                pending.ServiceId,
                pending.WorkerId,
                pending.CorrelationId,
                null,
                null,
                null,
                false,
                body);
            await eventBus.PublishAsync(@event, ct);
            return true;
        }

        /// <summary>
        /// Create or return the one already created.
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEventBus> CreateOrGetAsync(PartnerValues partnerId, CancellationToken ct = default)
        {
            var settings = _options[partnerId];
            var notificationSettings = settings.Notification;

            var type = _options[partnerId].Notification.Type;
            if (type != NotificationType.AzureEventBus)
                throw new NotSupportedException("This factory only create object of type AzureEventBus");

            if (_cache.TryGetValue(partnerId, out var bucket) && bucket.Prop1 == notificationSettings.Prop1 && bucket.Endpoint == notificationSettings.Endpoint)
                return bucket.AzureEventBus;

            // Critical section to avoid create 2 resource for the same partner.
            await _semaphore.WaitAsync(ct);
            try
            {
                if (_cache.TryGetValue(partnerId, out bucket) && bucket.Prop1 == notificationSettings.Prop1 && bucket.Endpoint == notificationSettings.Endpoint)
                    return bucket.AzureEventBus;

                _logger.LogInformation("Create azure bus for partner: {Id", partnerId);
                if (bucket?.AzureEventBus is IAsyncDisposable disposable)
                    await disposable.DisposeAsync();

                var queues = JsonUtility.Deserialize<string[]>(notificationSettings.Prop1)
                    .Select(s => new QueueAlias<FakeAlias>
                    {
                        Active = true,
                        Alias = (FakeAlias)(-1),
                        Queue = s
                    });
                var bus = new AzureEventBus<FakeAlias>(
                    notificationSettings.Endpoint,
                    queues,
                    null,
                    logger: _busLogger
                );
                await bus.StartAsync(TimeSpan.FromSeconds(5), ct);

                _cache.Remove(partnerId);
                _cache.Add(partnerId, bucket = new Bucket { AzureEventBus = bus, Prop1 = notificationSettings.Prop1, Endpoint = notificationSettings.Endpoint });
                return bucket.AzureEventBus;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #region Nested Classes
        public enum FakeAlias { }
        private sealed class Bucket
        {
            public string Prop1;
            public string Endpoint;
            public IEventBus AzureEventBus;
        }
        #endregion
    }
}
