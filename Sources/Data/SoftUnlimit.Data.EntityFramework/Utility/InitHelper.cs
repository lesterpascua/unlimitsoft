using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework.Utility
{
    /// <summary>
    /// Helper to initialize the app.
    /// </summary>
    public static class InitHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TUnitOfWork"></typeparam>
        /// <param name="factory"></param>
        /// <param name="setup"></param>
        /// <param name="eventBus"></param>
        /// <param name="seedAssemblies"></param>
        /// <param name="eventListener"></param>
        /// <param name="publishWorker"></param>
        /// <param name="loadEvent"></param>
        /// <param name="waitRetry"></param>
        /// <param name="logger"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task InitAsync<TUnitOfWork>(
            IServiceScopeFactory factory, Func<IServiceProvider, Task> setup = null, bool eventBus = false, Assembly[] seedAssemblies = null,
            bool eventListener = false, bool publishWorker = false, bool loadEvent = false, TimeSpan? waitRetry = null, ILogger logger = null, CancellationToken ct = default)
        {
            using var scope = factory.CreateScope();

            var provider = scope.ServiceProvider;
            var unitOfWorkType = typeof(TUnitOfWork);
            var unitOfWork = (IUnitOfWork)provider.GetRequiredService(unitOfWorkType);

            await EntityBuilderUtility.ExecuteSeedAndMigrationAsync(provider, unitOfWorkType: unitOfWorkType, assemblies: seedAssemblies, logger: logger, ct: ct);
            if (setup is not null)
                await setup.Invoke(provider);

            waitRetry ??= TimeSpan.FromSeconds(5);
            if (eventBus)
            {
                await provider.GetService<IEventBus>().StartAsync(waitRetry.Value, ct);
                if (publishWorker)
                    await provider.GetService<IEventPublishWorker>().StartAsync(loadEvent, ct);
            }
            if (eventListener)
                await provider.GetService<IEventListener>().ListenAsync(waitRetry.Value, ct);
        }
    }
}
