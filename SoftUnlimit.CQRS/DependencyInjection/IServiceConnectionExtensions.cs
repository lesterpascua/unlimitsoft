using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftUnlimit.CQRS.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class IServiceConnectionExtensions
    {
        /// <summary>
        /// Regiter all CQRS types.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        public static void AddSoftUnlimitDefaultCQRS(this IServiceCollection services, CQRSSettings settings)
        {
            services.RegisterQueryHandler(settings);
            services.RegisterCommandHandlerAndValidator(settings);
            services.RegisterEventHandler(settings);
        }

        #region Private Methods
        /// <summary>
        /// Register all querie services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        private static void RegisterQueryHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IQueryAsyncHandler != null && settings.IQueryAsyncHandlerGeneric != null)
            {
                services.AddScoped<IQueryAsyncDispatcher>(provider => new ServiceProviderQueryAsyncDispatcher(provider, true));
                CacheDispatcher.RegisterHandler(services, settings.Assemblies, settings.IQueryAsyncHandler, settings.IQueryAsyncHandlerGeneric);
            }
        }
        /// <summary>
        /// Register all commandHandlers, validator, compliance, eventHandlers, eventSourcedMediator.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        private static void RegisterCommandHandlerAndValidator(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.MediatorDispatchEventSourced != null)
                services.AddScoped(typeof(IMediatorDispatchEventSourced), settings.MediatorDispatchEventSourced);
            if (settings.ICommandCompliance != null)
                ServiceProviderCommandDispatcher.RegisterCommandCompliance(services, settings.ICommandCompliance, settings.Assemblies);
            if (settings.ICommandHandler != null)
            {
                ServiceProviderCommandDispatcher.RegisterCommandHandler(services, settings.ICommandHandler, settings.Assemblies, settings.Assemblies);

                services.AddSingleton<ICommandDispatcher>((provider) => {
                    return new ServiceProviderCommandDispatcher(
                        provider,
                        errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms
                    );
                });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static IServiceCollection RegisterEventHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IEventHandler != null)
            {
                ServiceProviderEventDispatcher.RegisterEventHandler(services, settings.IEventHandler, settings.Assemblies.ToArray());
                if (settings.EventDispatcher != null)
                {
                    services.AddSingleton<IEventDispatcher>((provider) => provider.GetService<IEventDispatcherWithServiceProvider>());
                    services.AddSingleton(settings.EventDispatcher);
                }
            }
            return services;
        }
        #endregion
    }
}
