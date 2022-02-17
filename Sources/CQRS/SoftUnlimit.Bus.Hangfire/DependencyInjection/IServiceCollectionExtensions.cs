using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Bus.Hangfire.Activator;
using SoftUnlimit.Bus.Hangfire.Filter;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading.Tasks;

namespace SoftUnlimit.Bus.Hangfire.DependencyInjection
{
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
        /// <returns></returns>
        public static IServiceCollection AddHangfireCommandBus(this IServiceCollection services,
            HangfireOptions options,
            string errorCode = null,
            Func<IServiceProvider, ICommand, Task> preeSendCommand = null,
            Action<IServiceProvider, ICommand, BackgroundJob> preeProcessCommand = null,
            Func<IServiceProvider, Exception, Task> onError = null,
            bool addLoggerFilter = false,
            Func<IServiceProvider, JobActivator> activatorFactory = null,
            Func<IServiceProvider, IServiceProvider> providerFactory = null,
            Action<IGlobalConfiguration> setup = null
        )
        {
            services
                .AddHangfire((provider, config) =>
                {
                    if (providerFactory is not null)
                        provider = providerFactory(provider);

                    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                    config.UseSimpleAssemblyNameTypeSerializer();
                    config.UseRecommendedSerializerSettings();

                    if (addLoggerFilter)
                        config.UseFilter(new LogEverythingAttribute(provider.GetService<ILogger<LogEverythingAttribute>>()));

                    var activator = activatorFactory?.Invoke(provider);
                    config.UseActivator(activator ?? new DefaultJobActivator(ActivatorUtilities.GetServiceOrCreateInstance<IServiceScopeFactory>(provider)));

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
                    Func<Exception, Task> interOnError = null;
                    if (onError is not null)
                        interOnError = exc => onError(provider, exc);

                    var dispatcher = provider.GetRequiredService<ICommandDispatcher>();
                    var logger = provider.GetRequiredService<ILogger<DefaultJobProcessor>>();
                    var commandCompletionService = provider.GetService<ICommandCompletionService>();

                    return new DefaultJobProcessor(
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
                    var client = provider.GetService<IBackgroundJobClient>();
                    var logger = provider.GetService<ILogger<HangfireCommandBus>>();

                    Func<ICommand, Task> innerPreeSendCommand = null;
                    if (preeSendCommand is not null)
                        innerPreeSendCommand = command => preeSendCommand(provider, command);

                    return new HangfireCommandBus(client, innerPreeSendCommand, logger);
                });

            return services;
        }
    }
}
