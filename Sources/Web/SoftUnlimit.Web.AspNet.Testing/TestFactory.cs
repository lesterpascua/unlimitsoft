using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Security;
using System;

namespace SoftUnlimit.Web.AspNet.Testing
{
    public static class TestFactory
    {
        /// <summary>
        /// Replace event bus.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection ReplaceEventBus(this IServiceCollection services)
        {
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
            services.RemoveAll<IEventListener>();
            services.AddSingleton(provider =>
            {
                var resolver = provider.GetService<IEventNameResolver>();
                var eventDispatcher = provider.GetService<IEventDispatcher>();
                return new ListenerFake(eventDispatcher, resolver);
            });
            services.AddSingleton<IEventListener>(p => p.GetService<ListenerFake>());

            return services;
        }

        /// <summary>
        /// Replace event listener for fake listener
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection ReplaceEventPublishWorker(this IServiceCollection services, Func<IServiceProvider, IEventPublishWorker> factory)
        {
            services.RemoveAll<IEventPublishWorker>();
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
        public static IServiceCollection ReplaceDbContextForInMemory<TDbContextRead, TDbContextWrite>(this IServiceCollection services, string name = null)
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
        public static WebApplicationFactory<TStartup> Factory<TStartup>(bool removeHostedService = true, Action<IServiceCollection> setup = null)
            where TStartup : class
        {
            var appFactory = new WebApplicationFactory<TStartup>()
                .WithWebHostBuilder(builder =>
                {
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
}
