using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Command.Pipeline;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
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
        public static void AddUnlimitSoftCQRS(this IServiceCollection services, CQRSSettings settings)
        {
            services.AddQueryHandler(settings);
            services.AddCommandHandler(settings);
            services.AddEventHandler(settings);
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
        public static void AddQueryHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IQueryHandler is not null)
                AddQueryHandler(services, settings.IQueryHandler, settings.PreeDispatchQuery, true, settings.Assemblies);
        }
        /// <summary>
        /// Scan assemblies and register all QueryHandler implement the interfaces set in <see cref="CQRSSettings.IQueryHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="queryHandlerType"></param>
        /// <param name="preeDispatch"></param>
        /// <param name="validate"></param>
        /// <param name="assemblies"></param>
        public static void AddQueryHandler(this IServiceCollection services, Type queryHandlerType, Action<IServiceProvider, IQuery> preeDispatch = null, bool validate = true, params Assembly[] assemblies)
        {
            services.AddScoped<IQueryDispatcher>((provider) =>
            {
                var logger = provider.GetService<ILogger<ServiceProviderQueryDispatcher>>();
                return new ServiceProviderQueryDispatcher(
                    provider,
                    errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms,
                    preeDispatch: preeDispatch,
                    validate: validate,
                    logger: logger
                );
            });

            #region Assembly Scan
            var existHandler = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IQueryHandler)));

            foreach (var handlerImplementation in existHandler)
            {
                var queryHandlerImplementedInterfaces = handlerImplementation
                    .GetInterfaces()
                    .Where(p => p.GetGenericArguments().Length == 2 && p.GetGenericTypeDefinition() == queryHandlerType);

                foreach (var queryHandlerInterface in queryHandlerImplementedInterfaces)
                {
                    var argsTypes = queryHandlerInterface.GetGenericArguments();
                    var handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(argsTypes);
                    var currHandlerInterface = queryHandlerType.MakeGenericType(argsTypes);

                    services.AddScoped(handlerInterface, handlerImplementation);
                    if (currHandlerInterface != handlerInterface)
                        services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));
                }
            }
            #endregion
        }

        /// <summary>
        /// Scan assemblies and register all CommandHandler implement the interfaces set in <see cref="CQRSSettings.ICommandHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        public static IServiceCollection AddCommandHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.ICommandHandler is not null)
                AddCommandHandler(services, settings.ICommandHandler, settings.PreeDispatchCommand, true, settings.Assemblies);
            return services;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="commandHandlerType"></param>
        /// <param name="preeDispatch"></param>
        /// <param name="validate"></param>
        /// <param name="assemblies"></param>
        public static IServiceCollection AddCommandHandler(this IServiceCollection services, Type commandHandlerType, Action<IServiceProvider, ICommand> preeDispatch = null, bool validate = true, params Assembly[] assemblies)
        {
            services.AddSingleton<ICommandDispatcher>((provider) =>
            {
                var logger = provider.GetService<ILogger<ServiceProviderCommandDispatcher>>();
                return new ServiceProviderCommandDispatcher(
                    provider,
                    errorTransforms: ServiceProviderCommandDispatcher.DefaultErrorTransforms,
                    preeDispatch: preeDispatch,
                    validate: validate,
                    logger: logger
                );
            });

            #region Assembly Scan
            var existHandler = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(ICommandHandler)));

            foreach (var handlerImplementation in existHandler)
            {
                var commandHandlerImplementedInterfaces = handlerImplementation
                    .GetInterfaces()
                    .Where(p => p.GetGenericArguments().Length == 1 && p.GetGenericTypeDefinition() == commandHandlerType);

                foreach (var commandHandlerInterface in commandHandlerImplementedInterfaces)
                {
                    var argsTypes = commandHandlerInterface.GetGenericArguments();
                    var handlerInterface = typeof(ICommandHandler<>).MakeGenericType(argsTypes);
                    var currHandlerInterface = commandHandlerType.MakeGenericType(argsTypes);

                    services.AddScoped(handlerInterface, handlerImplementation);
                    if (currHandlerInterface != handlerInterface)
                        services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));

                    // Post Pipelines
                    var attrs = argsTypes[0].GetCustomAttributes(typeof(PostPipelineAttribute), true);
                    if (attrs?.Any() == true)
                    {
                        var postPipelineHandlerTypes = attrs
                            .Cast<PostPipelineAttribute>()
                            .Select(s => s.Pipeline)
                            .ToArray();
                        foreach (var postPipelineHandlerType in postPipelineHandlerTypes)
                        {
                            var interfaces = postPipelineHandlerType
                                .GetInterfaces()
                                .Where(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ICommandHandlerPostPipeline<,,>));
                            foreach (var @interface in interfaces)
                            {
                                var args = @interface.GetGenericArguments();
                                var interfaceType = typeof(ICommandHandlerPostPipeline<,,>).MakeGenericType(args);
                                services.AddScoped(interfaceType, postPipelineHandlerType);
                            }
                        }

                    }
                }
            }
            #endregion

            return services;
        }
        
        /// <summary>
        /// Scan assemblies and register all EventHandler implement the interfaces set in <see cref="CQRSSettings.IEventHandler"/>
        /// </summary>
        /// <param name="services"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventHandler(this IServiceCollection services, CQRSSettings settings)
        {
            if (settings.IEventHandler is not null)
                AddEventHandler(services, settings.IEventHandler, settings.PreeDispatchEvent, settings.Assemblies);
            return services;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="eventHandlerType"></param>
        /// <param name="preeDispatch"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AddEventHandler(this IServiceCollection services, Type eventHandlerType, Action<IServiceProvider, IEvent> preeDispatch = null, params Assembly[] assemblies)
        {
            services.AddSingleton<IEventDispatcher>((provider) =>
            {
                var logger = provider.GetService<ILogger<ServiceProviderEventDispatcher>>();
                return new ServiceProviderEventDispatcher(
                    provider,
                    preeDispatch: preeDispatch,
                    logger: logger
                );
            });

            #region Assembly Scan
            var existHandler = assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IEventHandler)));

            foreach (var handlerImplementation in existHandler)
            {
                var eventHandlerImplementedInterfaces = handlerImplementation
                    .GetInterfaces()
                    .Where(p => p.GetGenericArguments().Length == 1 && p.GetGenericTypeDefinition() == eventHandlerType);

                foreach (var eventHandlerInterface in eventHandlerImplementedInterfaces)
                {
                    var argsTypes = eventHandlerInterface.GetGenericArguments().Single();
                    var handlerInterface = typeof(IEventHandler<>).MakeGenericType(argsTypes);
                    var currHandlerInterface = eventHandlerType.MakeGenericType(argsTypes);

                    services.AddScoped(handlerInterface, handlerImplementation);
                    if (currHandlerInterface != handlerInterface)
                        services.AddScoped(currHandlerInterface, provider => provider.GetService(handlerInterface));
                }
            }
            #endregion

            return services;
        }
    }
}
