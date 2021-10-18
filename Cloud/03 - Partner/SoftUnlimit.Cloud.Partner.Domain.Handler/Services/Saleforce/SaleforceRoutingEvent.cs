using SoftUnlimit.Cloud.Partner.Data.Model;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Saleforce.Sender.Model;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce
{
    /// <summary>
    /// 
    /// </summary>
    public class SaleforceRoutingEvent : IRoutingEvent
    {
        private readonly IEventPublisherApiServiceFactory _factory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public SaleforceRoutingEvent(IEventPublisherApiServiceFactory factory)
        {
            _factory = factory;
        }

        /// <inheritdoc />
        public NotificationType Type => NotificationType.SalesforcePlatformEvent;
        /// <inheritdoc />
        public async Task<bool> RouteAsync(PartnerValues partnerId, Pending pending, CancellationToken ct = default)
        {
            var saleforcePublishEventService = await _factory.CreateOrGetAsync(partnerId, ct);
            var vm = new EventSignature(pending.EventId, Guid.Parse(pending.SourceId), pending.Body, pending.Name);

            var (response, _) = await saleforcePublishEventService.PublishAsync(vm, ct);
            return response.Success;
        }
    }
}
