using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using UnlimitSoft.CQRS.Event;
using System;
using System.Linq;
using UnlimitSoft.Json;
using Microsoft.Extensions.Logging;
using UnlimitSoft.Distribute;

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

        services.AddSingleton<IEventBus>(provider => provider.GetService<EventBusFake>());
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
        services.AddSingleton<IEventListener>(p => p.GetService<ListenerFake>());

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
        }
        else
            services.AddSingleton(factory);

        return services;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDbContextRead"></typeparam>
    /// <typeparam name="TDbContextWrite"></typeparam>
    /// <param name="services"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IServiceCollection ReplaceDbContextForInMemory<TDbContextRead, TDbContextWrite>(this IServiceCollection services, string? name = null)
        where TDbContextRead : DbContext
        where TDbContextWrite : DbContext
    {
        services.RemoveAll<TDbContextRead>();
        services.RemoveAll<DbContextOptions>();
        services.RemoveAll<DbContextOptions<TDbContextRead>>();
#pragma warning disable EF1001 // Internal EF Core API usage.
        services.RemoveAll<IDbContextPool<TDbContextRead>>();
        services.RemoveAll<IScopedDbContextLease<TDbContextRead>>();
#pragma warning restore EF1001 // Internal EF Core API usage.

        services.RemoveAll<TDbContextWrite>();
        services.RemoveAll<DbContextOptions>();
        services.RemoveAll<DbContextOptions<TDbContextWrite>>();
#pragma warning disable EF1001 // Internal EF Core API usage.
        services.RemoveAll<IDbContextPool<TDbContextWrite>>();
        services.RemoveAll<IScopedDbContextLease<TDbContextWrite>>();
#pragma warning restore EF1001 // Internal EF Core API usage.

        var dbName = name;
        if (string.IsNullOrEmpty(name))
            dbName = Guid.NewGuid().ToString();

        var inMemoryDatabaseRoot = new InMemoryDatabaseRoot();
        services.AddDbContext<TDbContextRead>(options =>
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).UseInMemoryDatabase(dbName, inMemoryDatabaseRoot);
            options.ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
        });
        services.AddDbContext<TDbContextWrite>(options =>
        {
            options.UseInMemoryDatabase(dbName, inMemoryDatabaseRoot);
            options.ConfigureWarnings(x => x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning));
        });

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
