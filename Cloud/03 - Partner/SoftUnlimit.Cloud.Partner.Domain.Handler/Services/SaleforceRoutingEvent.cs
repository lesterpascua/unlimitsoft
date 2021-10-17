using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Configuration;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Services;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class SaleforceRoutingEvent : IRoutingEvent
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly PartnerOptions _options;
        private readonly ILogger<SaleforceRoutingEvent> _logger;
        private readonly Dictionary<PartnerValues, Bucket> _cache = new();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public SaleforceRoutingEvent(
            IOptions<PartnerOptions> options, 
            ILogger<SaleforceRoutingEvent> logger)
        {
            _options = options.Value;
            _logger = logger;
            _semaphore = new SemaphoreSlim(1, 1);
        }

        /// <inheritdoc />
        public NotificationType Type => NotificationType.SalesforcePlatformEvent;
        /// <inheritdoc />
        public async Task<bool> RouteAsync(PartnerValues partnerId, Pending pending, CancellationToken ct = default)
        {
            var saleforcePublishEventService = await CreateOrGetAsync(partnerId, ct);
            var vm = new EventSignature(pending.EventId, Guid.Parse(pending.SourceId), pending.Body, pending.Name);

            var (response, _) = await saleforcePublishEventService.PublishAsync(vm, ct);
            return response.Success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<IEventPublisherApiService> CreateOrGetAsync(PartnerValues partnerId, CancellationToken ct = default)
        {
            var settings = _options[partnerId];
            var notificationSettings = settings.Notification;

            var type = _options[partnerId].Notification.Type;
            if (type != NotificationType.SalesforcePlatformEvent)
                throw new NotSupportedException("This factory only create object of type AzureEventBus");

            if (_cache.TryGetValue(partnerId, out var bucket) && bucket.Equal(notificationSettings))
                return bucket.EventPublisher;

            // Critical section to avoid create 2 resource for the same partner.
            await _semaphore.WaitAsync(ct);
            try
            {
                if (_cache.TryGetValue(partnerId, out bucket) && bucket.Equal(notificationSettings))
                    return bucket.EventPublisher;

                _logger.LogInformation("Create SalesforcePlatformEvent instance for partner: {Id}", partnerId);

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory");

                httpClient.BaseAddress = new Uri(notificationSettings.Endpoint);
                var apiClient = new DefaultApiClient(httpClient);

                var options = new SaleforceOption
                {
                    ClientId = notificationSettings.ClientId,
                    UserName = notificationSettings.Prop1,
                    Secret = notificationSettings.Secret,
                    Password = notificationSettings.Prop2,
                    EventName = notificationSettings.Prop3
                };
                var auth = new AuthApiService(apiClient, new OptionsWrapper<SaleforceOption>(options));
                var service = new EventPublisherApiService(apiClient, auth);

                _cache.Remove(partnerId);
                _cache.Add(partnerId, bucket = new Bucket
                {
                    EventPublisher = service,
                    Prop1 = notificationSettings.Prop1,
                    Prop2 = notificationSettings.Prop2,
                    Prop3 = notificationSettings.Prop3,
                    ClientId = notificationSettings.ClientId,
                    LoginEndPoint = notificationSettings.Endpoint
                });
                return bucket.EventPublisher;
            }
            finally
            {
                _semaphore.Release();
            }
        }


        #region Nested Classes
        private sealed class Bucket
        {
            public string Prop1;
            public string Prop2;
            public string Prop3;
            public string ClientId;
            public string LoginEndPoint;

            public IEventPublisherApiService EventPublisher;

            public bool Equal(PartnerOptions.Notification settings) => settings.Prop1 == Prop1 && settings.Prop2 == Prop2 && settings.Prop3 == Prop3 && settings.ClientId == ClientId && settings.Endpoint == LoginEndPoint;
        }
        #endregion
    }
}
