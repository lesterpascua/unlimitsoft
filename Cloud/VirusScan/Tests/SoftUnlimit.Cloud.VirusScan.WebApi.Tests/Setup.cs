using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Cloud.Event;
using SoftUnlimit.Cloud.VirusScan.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Web.AspNet.Testing;
using System;
using System.IO;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SoftUnlimit.Cloud.VirusScan.WebApi.Tests
{
    /// <summary>
    /// Setup the unit test environment.
    /// </summary>
    public static class Setup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Stream GetNonVirusFile() =>
            typeof(Setup).Assembly.GetManifestResourceStream("SoftUnlimit.Cloud.VirusScan.WebApi.Tests.Resources.non-virus.txt");

        /// <summary>
        /// Build in memory server.
        /// </summary>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static WebApplicationFactory<Startup> Factory(Action<IServiceCollection> setup = null)
        {
            var appFactory = TestFactory.Factory<Startup>(true, setup: services => {
                services.ReplaceEventBus();
                services.ReplaceEventListener();
                services.ReplaceEventPublishWorker(provider =>
                {
                    var eventBus = provider.GetService<IEventBus>();
                    var logger = provider.GetService<ILogger<CloudQueueEventPublishWorker<ICloudUnitOfWork>>>();
                    var scopeFactory = provider.GetService<IServiceScopeFactory>();

                    return new CloudQueueEventPublishWorker<ICloudUnitOfWork>(
                        scopeFactory,
                        eventBus,
                        MessageType.Event,
                        TimeSpan.Zero,
                        TimeSpan.Zero,
                        logger: logger
                    );
                });
                services.ReplaceDbContextForInMemory<DbContextRead, DbContextWrite>();

                setup?.Invoke(services);
            });

            return appFactory;
        }
    }
}
