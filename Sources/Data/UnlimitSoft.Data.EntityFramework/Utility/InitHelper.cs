using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;

namespace UnlimitSoft.Data.EntityFramework.Utility;


/// <summary>
/// Helper to initialize the app.
/// </summary>
public static class InitHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="unitOfWorkType"></param>
    /// <param name="setup"></param>
    /// <param name="eventBus"></param>
    /// <param name="seedAssemblies"></param>
    /// <param name="eventListener"></param>
    /// <param name="publishWorker"></param>
    /// <param name="loadEvent"></param>
    /// <param name="waitRetry"></param>
    /// <param name="eventBashSize">If publish worker is enable indicate the bach size use to load all the events</param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task InitAsync(
        IServiceScopeFactory factory, Type? unitOfWorkType, Func<IServiceProvider, Task>? setup = null, bool eventBus = false, Assembly[]? seedAssemblies = null,
        bool eventListener = false, bool publishWorker = false, bool loadEvent = false, TimeSpan? waitRetry = null, int eventBashSize = 1000, ILogger? logger = null, CancellationToken ct = default
    )
    {
        using var scope = factory.CreateScope();

        var provider = scope.ServiceProvider;
        if (unitOfWorkType is not null)
            await EntityBuilderUtility.ExecuteSeedAndMigrationAsync(provider, unitOfWorkType, seedAssemblies, logger: logger, ct: ct);
        if (setup is not null)
            await setup.Invoke(provider);

        waitRetry ??= TimeSpan.FromSeconds(5);
        if (eventBus)
        {
            await provider.GetRequiredService<IEventBus>().StartAsync(waitRetry.Value, ct);
            if (publishWorker)
                await provider.GetRequiredService<IEventPublishWorker>().StartAsync(loadEvent, eventBashSize, ct);
        }
        if (eventListener)
            await provider.GetRequiredService<IEventListener>().ListenAsync(waitRetry.Value, ct);
    }
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
    /// <param name="eventBashSize">If publish worker is enable indicate the bach size use to load all the events</param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task InitAsync<TUnitOfWork>(
        IServiceScopeFactory factory, Func<IServiceProvider, Task>? setup = null, bool eventBus = false, Assembly[]? seedAssemblies = null,
        bool eventListener = false, bool publishWorker = false, bool loadEvent = false, TimeSpan? waitRetry = null, int eventBashSize = 1000, ILogger? logger = null, CancellationToken ct = default
    )
        where TUnitOfWork : IUnitOfWork
    {
        var unitOfWorkType = typeof(TUnitOfWork);
        return InitAsync(factory, unitOfWorkType, setup, eventBus, seedAssemblies, eventListener, publishWorker, loadEvent, waitRetry, eventBashSize, logger, ct);
    }
}
