using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.Domain.Events;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;

namespace OIU.Libs.Domain.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Register all CQRS actions
    /// </summary>
    /// <param name="services"></param>
    /// <param name="useMemoryMediator"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services, bool useMemoryMediator, CQRSSettings options)
    {
        services.AddUnlimitSoftCQRS(options);

        if (!useMemoryMediator)
            services.AddSingleton<IEventPublishWorker, MemoryMediatorDispatchEvent.InternalEventPublishWorker>();

        return services;
    }
}
