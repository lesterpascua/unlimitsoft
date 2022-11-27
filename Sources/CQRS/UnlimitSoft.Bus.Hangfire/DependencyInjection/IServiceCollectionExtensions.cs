using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnlimitSoft.Bus.Hangfire.Activator;
using UnlimitSoft.Bus.Hangfire.Filter;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Web.Client;
using UnlimitSoft.Message;

namespace UnlimitSoft.Bus.Hangfire.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Create command bus using hangfire
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <param name="errorCode">Error code in case some exception happened. If null in the body will arrive the exception.</param>
    /// <param name="preeSendCommand">Invoke this function before send any command to the bus.</param>
    /// <param name="preeProcessCommand">Before sent the command to the dispatcher execute this function to custome add more information to the command.</param>
    /// <param name="onError"></param>
    /// <param name="addLoggerFilter"></param>
    /// <param name="providerFactory"></param>
    /// <param name="activatorFactory">Custom activator creator.</param>
    /// <param name="setup"></param>
    /// <param name="recomendedConfig"></param>
    /// <param name="compatibility">Default <see cref="CompatibilityLevel.Version_170"/>.</param>
    /// <returns></returns>
    public static IServiceCollection AddHangfireCommandBus<TProps>(this IServiceCollection services,
        HangfireOptions options,
        string? errorCode = null,
        Func<IServiceProvider, ICommand, Task>? preeSendCommand = null,
        Func<IServiceProvider, ICommand, JobActivatorContext, Func<ICommand, CancellationToken, Task<IResponse>>, CancellationToken, Task<IResponse>>? preeProcessCommand = null,
        Func<IServiceProvider, Exception, Task>? onError = null,
        bool addLoggerFilter = false,
        Func<IServiceProvider, JobActivator>? activatorFactory = null,
        Func<IServiceProvider, IServiceProvider>? providerFactory = null,
        Action<IGlobalConfiguration>? setup = null,
        bool recomendedConfig = true,
        CompatibilityLevel compatibility = CompatibilityLevel.Version_170
    )
        where TProps : CommandProps
    {
        services
            .AddHangfire((provider, config) =>
            {
                config.SetDataCompatibilityLevel(compatibility);
                if (providerFactory is not null)
                    provider = providerFactory(provider);

                if (recomendedConfig)
                {
                    config.UseSimpleAssemblyNameTypeSerializer();
                    config.UseRecommendedSerializerSettings();

                    if (addLoggerFilter)
                        config.UseFilter(new LogEverythingAttribute(provider.GetRequiredService<ILogger<LogEverythingAttribute>>()));

                    var activator = activatorFactory?.Invoke(provider);
                    config.UseActivator(activator ?? new DefaultJobActivator(ActivatorUtilities.GetServiceOrCreateInstance<IServiceScopeFactory>(provider)));
                }

                if (setup is null)
                {
                    config.UseSqlServerStorage(options.ConnectionString, new SqlServerStorageOptions
                    {
                        SchemaName = options.Scheme,
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true
                    });
                }
                else
                    setup(config);
            })
            .AddScoped<IJobProcessor>(provider =>
            {
                Func<Exception, Task>? interOnError = null;
                if (onError is not null)
                    interOnError = exc => onError(provider, exc);

                var dispatcher = provider.GetRequiredService<ICommandDispatcher>();
                var logger = provider.GetRequiredService<ILogger<DefaultJobProcessor<TProps>>>();
                var commandCompletionService = provider.GetService<ICommandCompletionService>();

                return new DefaultJobProcessor<TProps>(
                    provider,
                    dispatcher, 
                    errorCode: errorCode,
                    completionService: commandCompletionService,
                    onError: interOnError, 
                    preeProcess: preeProcessCommand,
                    logger: logger
                );
            })
            .AddSingleton<ICommandBus>(provider =>
            {
                var client = provider.GetRequiredService<IBackgroundJobClient>();
                var logger = provider.GetRequiredService<ILogger<HangfireCommandBus>>();

                Func<ICommand, Task>? innerPreeSendCommand = null;
                if (preeSendCommand is not null)
                    innerPreeSendCommand = command => preeSendCommand(provider, command);

                return new HangfireCommandBus(client, innerPreeSendCommand, logger: logger);
            });

        return services;
    }
}
