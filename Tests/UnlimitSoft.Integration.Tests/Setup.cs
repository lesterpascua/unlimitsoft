using Hangfire;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using UnlimitSoft.Bus.Hangfire;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Distribute;
using UnlimitSoft.Json;
using UnlimitSoft.Web.AspNet.Testing;
using UnlimitSoft.Web.Client;
using UnlimitSoft.WebApi.Sources.CQRS.Event;
using UnlimitSoft.WebApi.Sources.Security;
using Xunit.Abstractions;

namespace UnlimitSoft.Integration.Tests;


/// <summary>
/// Setup the unit test environment
/// </summary>
public static class Setup
{
    /// <summary>
    /// Build in memory server
    /// </summary>
    /// <remarks>
    ///     - Replace logger for empty or xunit if is set
    ///     - Replace EventBus for faker
    ///     - Replace EventPublishWorker to avoid start delay (increase test speed)
    ///     - Replace <see cref="IDbContextRead"/> and <see cref="IDbContextWrite"/> for in memory (increase test speed and reset environment every test)
    ///     - Replace <see cref="ISysLock"/> for a mocked
    ///     - Replace <see cref="ICommandBus"/> for a faker
    ///     - Replace <see cref="IBackgroundJobClient"/> for faker
    ///     - Replace <see cref="IRecurringJobManager"/> for faker
    /// </remarks>
    /// <param name="apiClient"></param>
    /// <param name="authorize"></param>
    /// <param name="setup">Extra setup arguments</param>
    /// <param name="useInMemContext">Indicate a memory db context.</param>
    /// <param name="output"></param>
    /// <param name="bus">If is null will mock with a faker.</param>
    /// <param name="isXUnitTest"></param>
    /// <param name="replaceEventBus"></param>
    /// <param name="replaceEventListener"></param>
    /// <param name="removeHostedService"></param>
    /// <returns></returns>
    public static WebApplicationFactory<TStartup> Factory<TStartup, TDbRead, TDbWrite>(
        out IApiClient apiClient,
        out AuthorizeOptions authorize,
        Action<IServiceCollection>? setup = null,
        bool useInMemContext = true,
        ITestOutputHelper? output = null,
        ICommandBus? bus = null,
        bool isXUnitTest = true,
        bool replaceEventBus = true,
        bool replaceEventListener = true,
        bool removeHostedService = true
    )
        where TStartup : class
        where TDbRead : DbContext
        where TDbWrite : DbContext
    {
        var appFactory = TestFactory.Factory<TStartup>(
            builder => builder.ConfigureAppConfiguration(
                (context, configBuilder) => { }
            ),
            removeHostedService,
            setup: services =>
            {
                if (useInMemContext)
                    services.ReplaceDbContextForInMemory(typeof(TDbRead), typeof(TDbWrite));

                if (replaceEventBus)
                    services.ReplaceEventBus();
                if (replaceEventListener)
                    services.ReplaceEventListener();

                services.ReplaceEventPublishWorker(provider =>
                {
                    var clock = provider.GetRequiredService<ISysClock>();
                    var factory = provider.GetRequiredService<IServiceScopeFactory>();
                    var eventBus = provider.GetRequiredService<IEventBus>();
                    var logger = provider.GetRequiredService<ILogger<MyQueueEventPublishWorker>>();

                    return new MyQueueEventPublishWorker(
                        clock,
                        factory,
                        eventBus,
                        startDelay: TimeSpan.Zero,
                        errorDelay: TimeSpan.Zero,
                        10,
                        logger: logger
                    );
                });

                services.RemoveAll<ISysLock>();
                services.AddSingleton(Substitute.For<ISysLock>());

                if (isXUnitTest)
                {
                    services.RemoveAll<ICommandBus>();
                    services.TryAddSingleton(bus ?? Substitute.For<ICommandBus>());
                    services.TryAddSingleton(Substitute.For<IJobProcessor>());
                    services.TryAddSingleton(Substitute.For<IBackgroundJobClient>());
                    services.TryAddSingleton(Substitute.For<IRecurringJobManager>());
                }

                setup?.Invoke(services);
            });

        apiClient = new DefaultApiClient(
            appFactory.CreateClient(),
            appFactory.Services.GetRequiredService<IJsonSerializer>()
        );
        authorize = appFactory.Services.GetRequiredService<IOptions<AuthorizeOptions>>().Value;

        return appFactory;
    }
}
