using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Command.Pipeline;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Event;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.DependencyInjection;


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
    public static IServiceCollection AddUnlimitSoftEventNameResolver(this IServiceCollection services, Assembly[] typeResolverCache, Func<Type, string>? transform = null)
    {
        var register = GetTypesFromInterface<IEvent>(typeResolverCache).ToDictionary(k => transform?.Invoke(k) ?? k.FullName!);
        return services.AddSingleton<IEventNameResolver>(new DefaultEventCommandResolver(register));

        // =================================================================================================================================
        static IEnumerable<Type> GetTypesFromInterface<T>(params Assembly[] assemblies) where T : class
        {
            if (!typeof(T).IsInterface)
                throw new InvalidOperationException($"Type {typeof(T)} must be an interface.");

            var result = new List<Type>();
            foreach (var assambly in assemblies)
            {
                var types = assambly
                    .GetTypes()
                    .Where(mytype => mytype.IsClass && !mytype.IsAbstract && mytype.GetInterfaces().Contains(typeof(T)));

                result.AddRange(types);
            }
            return result;
        }
    }
    /// <summary>
    /// Scan assemblies and register all QueryHandler implement the interfaces set in <see cref="CQRSSettings.IQueryHandler"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="settings"></param>
    public static void AddQueryHandler(this IServiceCollection services, CQRSSettings settings)
    {
        if (settings.IQueryHandler is not null)
            AddQueryHandler(services, settings.IQueryHandler, true, settings.Assemblies);
    }
    /// <summary>
    /// Scan assemblies and register all QueryHandler implement the interfaces set in <see cref="CQRSSettings.IQueryHandler"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="queryHandlerType"></param>
    /// <param name="validate"></param>
    /// <param name="assemblies"></param>
    public static void AddQueryHandler(this IServiceCollection services, Type queryHandlerType, bool validate = true, params Assembly[] assemblies)
    {
        services.AddSingleton<IQueryDispatcher>((provider) => new ServiceProviderQueryDispatcher(provider, validate: validate));

        #region Assembly Scan
        var existHandler = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IQueryHandler)));

        foreach (var handlerImplementation in existHandler)
        {
            var queryHandlerImplementedInterfaces = handlerImplementation
                .GetInterfaces()
                .Where(p =>
                {
                    if (p.GetGenericArguments().Length != 2)
                        return false;
                    return p.GetGenericTypeDefinition() == queryHandlerType;
                });

            foreach (var queryHandlerInterface in queryHandlerImplementedInterfaces)
            {
                var argsTypes = queryHandlerInterface.GetGenericArguments();
                var handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(argsTypes);
                var requestHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(argsTypes);
                var currHandlerInterface = queryHandlerType.MakeGenericType(argsTypes);

                services.AddScoped(handlerInterface, handlerImplementation);
                if (currHandlerInterface != handlerInterface)
                    services.AddScoped(currHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
                if (requestHandlerInterface != handlerInterface)
                    services.AddScoped(requestHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
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
            AddCommandHandler(services, settings.ICommandHandler, true, settings.Assemblies);
        return services;
    }
    /// <summary>
    /// Scan assemblies and register all CommandHandler implement the interfaces set in commandHandlerType
    /// </summary>
    /// <param name="services"></param>
    /// <param name="commandHandlerType"></param>
    /// <param name="validate"></param>
    /// <param name="assemblies"></param>
    public static IServiceCollection AddCommandHandler(this IServiceCollection services, Type commandHandlerType, bool validate = true, params Assembly[] assemblies)
    {
        services.AddSingleton<ICommandDispatcher>((provider) => new ServiceProviderCommandDispatcher(provider, validate: validate));

        #region Assembly Scan
        var existHandler = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(ICommandHandler)));

        foreach (var handlerImplementation in existHandler)
        {
            var commandHandlerImplementedInterfaces = handlerImplementation
                .GetInterfaces()
                .Where(p =>
                {
                    if (p.GetGenericArguments().Length != 2)
                        return false;
                    return p.GetGenericTypeDefinition() == commandHandlerType;
                });

            foreach (var commandHandlerInterface in commandHandlerImplementedInterfaces)
            {
                var argsTypes = commandHandlerInterface.GetGenericArguments();
                var handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(argsTypes);
                var requestHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(argsTypes);
                var currHandlerInterface = commandHandlerType.MakeGenericType(argsTypes);

                services.AddScoped(handlerInterface, handlerImplementation);
                if (currHandlerInterface != handlerInterface)
                    services.AddScoped(currHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
                if (requestHandlerInterface != handlerInterface)
                    services.AddScoped(requestHandlerInterface, provider => provider.GetRequiredService(handlerInterface));

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
            AddEventHandler(services, settings.IEventHandler, settings.Assemblies);
        return services;
    }
    /// <summary>
    /// Scan assemblies and register all EventHandler implement the type set in eventHandlerType
    /// </summary>
    /// <param name="services"></param>
    /// <param name="eventHandlerType"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddEventHandler(this IServiceCollection services, Type eventHandlerType, params Assembly[] assemblies)
    {
        services.AddSingleton<IEventDispatcher>((provider) => new ServiceProviderEventDispatcher(provider, true));

        #region Assembly Scan
        var existHandler = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsClass && !p.IsAbstract && p.GetInterfaces().Contains(typeof(IEventHandler)));

        foreach (var handlerImplementation in existHandler)
        {
            var eventHandlerImplementedInterfaces = handlerImplementation
                .GetInterfaces()
                .Where(p =>
                {
                    if (p.GetGenericArguments().Length != 1)
                        return false;
                    return p.GetGenericTypeDefinition() == eventHandlerType;
                });

            foreach (var eventHandlerInterface in eventHandlerImplementedInterfaces)
            {
                var argsTypes = eventHandlerInterface.GetGenericArguments();
                var handlerInterface = typeof(IEventHandler<>).MakeGenericType(argsTypes);
                var requestHandlerInterface = typeof(IRequestHandler<,>).MakeGenericType(argsTypes[0], typeof(IResponse));
                var currHandlerInterface = eventHandlerType.MakeGenericType(argsTypes);

                services.AddScoped(handlerInterface, handlerImplementation);
                if (currHandlerInterface != handlerInterface)
                    services.AddScoped(currHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
                if (requestHandlerInterface != handlerInterface)
                    services.AddScoped(requestHandlerInterface, provider => provider.GetRequiredService(handlerInterface));
            }
        }
        #endregion

        return services;
    }
}
