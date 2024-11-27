using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Distribute;
using UnlimitSoft.Json;

namespace UnlimitSoft.Web.AspNet.Testing;


/// <summary>
/// 
/// </summary>
public static class TestFactory
{
    /// <summary>
    /// Replace event bus.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceEventBus(this IServiceCollection services)
    {
        if (!services.Any(p => p.ServiceType == typeof(IEventBus)))
            return services;

        services.RemoveAll<IEventBus>();

        services.AddSingleton<IEventBus>(provider => provider.GetRequiredService<EventBusFake>());
        services.AddSingleton(provider => new EventBusFake());

        return services;
    }
    /// <summary>
    /// Replace event listener for fake listener
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceEventListener(this IServiceCollection services)
    {
        if (!services.Any(p => p.ServiceType == typeof(IEventListener)))
            return services;

        services.RemoveAll<IEventListener>();
        services.AddSingleton(provider =>
        {
            var resolver = provider.GetRequiredService<IEventNameResolver>();
            var eventDispatcher = provider.GetRequiredService<IEventDispatcher>();
            var serializer = provider.GetRequiredService<IJsonSerializer>();
            var logger = provider.GetService<ILogger<ListenerFake>>();

            return new ListenerFake(serializer, eventDispatcher, resolver, logger);
        });
        services.AddSingleton<IEventListener>(p => p.GetRequiredService<ListenerFake>());

        return services;
    }
    /// <summary>
    /// Replace event listener for fake listener
    /// </summary>
    /// <param name="services"></param>
    /// <param name="noLock"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceSysLock(this IServiceCollection services, bool noLock = true)
    {
        if (!services.Any(p => p.ServiceType == typeof(ISysLock)))
            return services;

        services.RemoveAll<ISysLock>();
        if (noLock)
        {
            services.AddSingleton<ISysLock>(SysLockFake.Instance);
            services.AddSingleton(p => p.GetRequiredService<SysLockFake>());
        }
        else
        {
            services.AddSingleton<ISysLock>(new SemaphoreSlimSysLock());
            services.AddSingleton(p => p.GetRequiredService<SemaphoreSlimSysLock>());
        }

        return services;
    }
    /// <summary>
    /// Replace event listener for fake listener
    /// </summary>
    /// <param name="services"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceEventPublishWorker(this IServiceCollection services, Func<IServiceProvider, IEventPublishWorker>? factory = null)
    {
        if (!services.Any(p => p.ServiceType == typeof(IEventPublishWorker)))
            return services;

        services.RemoveAll<IEventPublishWorker>();
        if (factory is null)
        {
            services.AddSingleton(provider => new EventPublishWorkerFake(provider.GetRequiredService<IEventBus>()));
            services.AddSingleton<IEventPublishWorker>(provider => provider.GetRequiredService<EventPublishWorkerFake>());
        }
        else
            services.AddSingleton(factory);

        return services;
    }

    /// <summary>
    /// Replace dbContext fore read and write for in memory database.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="dbContextRead"></param>
    /// <param name="dbContextWrite"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceDbContextForInMemory(this IServiceCollection services, Type? dbContextRead, Type? dbContextWrite, string? name = null)
    {
        name ??= Guid.NewGuid().ToString();
        var inMemoryDatabaseRoot = new InMemoryDatabaseRoot();

        if (dbContextRead is not null)
            services.ReplaceDbContextForInMemory(dbContextRead, false, inMemoryDatabaseRoot, name);
        if (dbContextWrite is not null)
            services.ReplaceDbContextForInMemory(dbContextWrite, true, inMemoryDatabaseRoot, name);
        return services;
    }
    /// <summary>
    /// Replace dbContext for in memory database.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="dbContext"></param>
    /// <param name="allowWrite"></param>
    /// <param name="inMemoryDatabaseRoot"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceDbContextForInMemory(this IServiceCollection services, Type dbContext, bool allowWrite, InMemoryDatabaseRoot? inMemoryDatabaseRoot = null, string? name = null)
    {
        services.RemoveAll(dbContext);
        services.RemoveAll<DbContextOptions>();
        services.RemoveAll(typeof(DbContextOptions<>).MakeGenericType(dbContext));
#if NET9_0_OR_GREATER
        services.RemoveAll(typeof(IDbContextOptionsConfiguration<>).MakeGenericType(dbContext));
#endif
#pragma warning disable EF1001 // Internal EF Core API usage.
        services.RemoveAll(typeof(IDbContextPool<>).MakeGenericType(dbContext));
        services.RemoveAll(typeof(IScopedDbContextLease<>).MakeGenericType(dbContext));
#pragma warning restore EF1001 // Internal EF Core API usage.

        Action<DbContextOptionsBuilder>? options = options =>
        {
            if (!allowWrite)
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseInMemoryDatabase(name, inMemoryDatabaseRoot);
            options.ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
        };
        typeof(EntityFrameworkServiceCollectionExtensions)
            .GetMethod(
                nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext),
                genericParameterCount: 1,
                bindingAttr: BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(IServiceCollection), typeof(Action<DbContextOptionsBuilder>), typeof(ServiceLifetime), typeof(ServiceLifetime)],
                modifiers: null
            )!
            .MakeGenericMethod(dbContext)
            .Invoke(null, [services, options, ServiceLifetime.Scoped, ServiceLifetime.Scoped]);

        return services;
    }

    /// <summary>
    /// Build in memory server.
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    /// <param name="removeHostedService"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static WebApplicationFactory<TStartup> Factory<TStartup>(bool removeHostedService = true, Action<IServiceCollection>? setup = null) where TStartup : class => Factory<TStartup>(null, removeHostedService, setup);
    /// <summary>
    /// Build in memory server.
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    /// <param name="setupBuilder"></param>
    /// <param name="removeHostedService"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    public static WebApplicationFactory<TStartup> Factory<TStartup>(Action<IWebHostBuilder>? setupBuilder, bool removeHostedService = true, Action<IServiceCollection>? setup = null)
        where TStartup : class
    {
        var appFactory = new WebApplicationFactory<TStartup>()
            .WithWebHostBuilder(builder =>
            {
                setupBuilder?.Invoke(builder);
                builder.ConfigureServices(services =>
                {
                    //services.ReplaceEventBus();
                    //services.ReplaceEventListener();
                    //services.ReplaceEventPublishWorker<TJnUnitOfWork>();
                    //services.ReplaceDbContextForInMemory<TDbContextRead, TDbContextWrite>();

                    if (removeHostedService)
                        services.RemoveAll<IHostedService>();

                    setup?.Invoke(services);
                });
            });

        return appFactory;
    }
}
