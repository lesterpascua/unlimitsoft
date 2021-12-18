using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

            #region Versioned Repository
            if (settings.IEventSourcedRepository is not null && settings.EventSourcedRepository is not null)
            {
                var constraints = settings.IEventSourcedRepository
                    .GetInterfaces()
                    .Any(i => i.GetGenericTypeDefinition() == typeof(IEventSourcedRepository<,>));
                if (!constraints)
                    throw new InvalidOperationException("IVersionedEventRepository don't implement IEventSourcedRepository<TVersionedEventPayload, TPayload>");
                
                services.AddScoped(settings.IEventSourcedRepository, settings.EventSourcedRepository);
            }
            #endregion


            services.RegisterQueryHandler(settings);
            services.RegisterCommandHandler(settings);
            services.RegisterEventHandler(settings);
        }
        /// <summary>
        /// Scan assemblies and register all events.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="typeResolverCache"></param>
        /// <param name="transform">Allow create custom name to the event.</param>
        /// <returns></returns>
        public static IServiceCollection AddSoftUnlimitEventNameResolver(this IServiceCollection services, Assembly[] typeResolverCache, Func<Type, string> transform = null)
        {
            static IEnumerable<Type> GetTypesFromInterface<T>(params Assembly[] assemblies) where T : class
            {
                if (!typeof(T).IsInterface)
                    throw new InvalidOperationException($"Type {typeof(T)} must be an interface.");

                var result = new List<Type>();
                foreach (var assambly in assemblies)
                {
                    var types = assambly
                        .GetTypes()
                        .Where(mytype => mytype.GetInterfaces().Contains(typeof(T)));

                    result.AddRange(types);
                }
                return result;
            }

            var register = GetTypesFromInterface<IEvent>(typeResolverCache).ToDictionary(k => transform?.Invoke(k) ?? k.FullName);
            return services.AddSingleton<IEventNameResolver>(new DefaultEventCommandResolver(register));
        }

        /// <summary>
        /// Scan assemblies and register all QueryHandler implement the interfaces set in <see cref="CQRSSettings.IQueryHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        public static void RegisterQueryHandler(this IServiceCollection services, CQRSSettings settings)
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
        public static void RegisterCommandHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.MediatorDispatchEventSourced is not null)
                services.AddScoped(typeof(IMediatorDispatchEventSourced), settings.MediatorDispatchEventSourced);
           
            if (settings.ICommandHandler is not null)
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
        public static IServiceCollection RegisterEventHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IEventHandler is not null)
            {
                if (settings.EventDispatcher is not null)
                    services.AddSingleton(settings.EventDispatcher);

                #region Assembly Scan
                var existHandler = settings.Assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IEventHandler)));

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

                        services.AddScoped(handlerInterface, handlerImplementation);
                        if (currHandlerInterface != handlerInterface)
                            services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));
                    }
                }
                #endregion
            }
            return services;
        }
    }
}
