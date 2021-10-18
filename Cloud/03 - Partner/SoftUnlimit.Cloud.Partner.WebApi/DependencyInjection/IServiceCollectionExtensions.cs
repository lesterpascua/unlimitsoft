using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Configuration;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services.AzureEventBus;
using SoftUnlimit.Cloud.Partner.Domain.Handler.Services.Saleforce;
using SoftUnlimit.Cloud.Partner.WebApi.Background;

namespace SoftUnlimit.Cloud.Partner.WebApi.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddRoutingEvent(this IServiceCollection services, PartnerOptions options)
        {
            services.AddSingleton<IRoutingEvent, SaleforceRoutingEvent>();
            services.AddSingleton<IEventPublisherApiServiceFactory, EventPublisherApiServiceFactory>();

            services.AddSingleton<IRoutingEvent, AzureEventBusRoutingEvent>();

            if (options.TryGetValue(PartnerValues.Saleforce, out var partner) && partner.Enable)
                services.AddHostedService<SaleforceBackground>();
            if (options.TryGetValue(PartnerValues.JnReward, out partner) && partner.Enable)
                services.AddHostedService<JnRewardBackground>();

            return services;
        }
    }
}
