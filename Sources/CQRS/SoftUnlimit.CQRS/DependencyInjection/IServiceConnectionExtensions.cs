using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Query;
using System;
using System.Collections.Generic;
using System.Linq;

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
            services.RegisterCommandHandler(settings);
            services.RegisterEventHandler(settings);
        }

        #region Private Methods
        /// <summary>
        /// Scan assemblies and register all QueryHandler implement the interfaces set in <see cref="CQRSSettings.IQueryHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        private static void RegisterQueryHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IQueryHandler != null)
            {
                services.AddScoped<IQueryDispatcher>(provider => new ServiceProviderQueryDispatcher(provider));

                #region Assembly Scan
                var existHandler = settings.Assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IQueryHandler)));

                foreach (var handlerImplementation in existHandler)
                {
                    var queryHandlerImplementedInterfaces = handlerImplementation
                        .GetInterfaces()
                        .Where(p => p.GetGenericArguments().Length == 2 && p.GetGenericTypeDefinition() == settings.IQueryHandler);

                    foreach (var queryHandlerInterface in queryHandlerImplementedInterfaces)
                    {
                        var argsTypes = queryHandlerInterface.GetGenericArguments();
                        var handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(argsTypes);
                        var currHandlerInterface = settings.IQueryHandler.MakeGenericType(argsTypes);

                        services.AddScoped(handlerInterface, handlerImplementation);
                        if (currHandlerInterface != handlerInterface)
                            services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// Scan assemblies and register all CommandHandler implement the interfaces set in <see cref="CQRSSettings.ICommandHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        private static void RegisterCommandHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.MediatorDispatchEventSourced != null)
                services.AddScoped(typeof(IMediatorDispatchEventSourced), settings.MediatorDispatchEventSourced);
           
            if (settings.ICommandHandler != null)
            {
                services.AddSingleton<ICommandDispatcher>((provider) => {
                    var logger = provider.GetService<ILogger<ServiceProviderCommandDispatcher>>();
                    return new ServiceProviderCommandDispatcher(
                        provider,
                        errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms,
                        preeDispatch: settings.PreeDispatchAction, 
                        logger: logger
                    );
                });

                #region Assembly Scan
                var existHandler = settings.Assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(ICommandHandler)));

                foreach (var handlerImplementation in existHandler)
                {
                    var commandHandlerImplementedInterfaces = handlerImplementation
                        .GetInterfaces()
                        .Where(p => p.GetGenericArguments().Length == 1 && p.GetGenericTypeDefinition() == settings.ICommandHandler);

                    foreach (var commandHandlerInterface in commandHandlerImplementedInterfaces)
                    {
                        var argsTypes = commandHandlerInterface.GetGenericArguments();
                        var handlerInterface = typeof(ICommandHandler<>).MakeGenericType(argsTypes);
                        var currHandlerInterface = settings.ICommandHandler.MakeGenericType(argsTypes);

                        services.AddScoped(handlerInterface, handlerImplementation);
                        if (currHandlerInterface != handlerInterface)
                            services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// Scan assemblies and register all EventHandler implement the interfaces set in <see cref="CQRSSettings.IEventHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static IServiceCollection RegisterEventHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IEventHandler != null)
            {
                if (settings.EventDispatcher != null)
                    services.AddSingleton(settings.EventDispatcher);

                #region Assembly Scan
                var existHandler = settings.Assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IEventHandler)));

                var register = new Dictionary<string, Type>();
                foreach (var handlerImplementation in existHandler)
                {
                    var eventHandlerImplementedInterfaces = handlerImplementation
                        .GetInterfaces()
                        .Where(p => p.GetGenericArguments().Length == 1 && p.GetGenericTypeDefinition() == settings.IEventHandler);

                    foreach (var eventHandlerInterface in eventHandlerImplementedInterfaces)
                    {
                        var argsTypes = eventHandlerInterface.GetGenericArguments().Single();
                        var handlerInterface = typeof(IEventHandler<>).MakeGenericType(argsTypes);
                        var currHandlerInterface = settings.IEventHandler.MakeGenericType(argsTypes);

                        if (!register.ContainsKey(argsTypes.FullName))
                            register.Add(argsTypes.FullName, argsTypes);

                        services.AddScoped(handlerInterface, handlerImplementation);
                        if (currHandlerInterface != handlerInterface)
                            services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));
                    }
                }
                #endregion

                services.AddSingleton<IEventNameResolver>(new DefaultEventCommandResolver(register));
            }
            return services;
        }
        #endregion
    }
}
