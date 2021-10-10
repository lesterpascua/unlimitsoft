using Hangfire;
using Hangfire.SqlServer;
using SoftUnlimit.Bus.Hangfire.Activator;
using SoftUnlimit.Bus.Hangfire.Filter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IServiceCollection AddHangfireCommandBus(this IServiceCollection services,
            HangfireOptions options,
            string errorCode = null,
            Func<IServiceProvider, ICommand, Task> preeSendCommand = null,
            Action<ICommand, BackgroundJob> preeProcessCommand = null,
            Func<IServiceProvider, Exception, Task> onError = null,
            bool addLoggerFilter = false,
            Action<IGlobalConfiguration> setup = null)
        {
            services
                .AddHangfire((provider, config) =>
                {
                    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                    config.UseSimpleAssemblyNameTypeSerializer();
                    config.UseRecommendedSerializerSettings();

                    if (addLoggerFilter)
                        config.UseFilter(new LogEverythingAttribute(provider.GetService<ILogger<LogEverythingAttribute>>()));
                    config.UseActivator(new DefaultJobActivator(ActivatorUtilities.GetServiceOrCreateInstance<IServiceScopeFactory>(provider)));
                    config.UseColouredConsoleLogProvider(options.Logger);

                    if (setup == null)
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
                    if (onError != null)
                        interOnError = exc => onError(provider, exc);

                    var dispatcher = provider.GetService<ICommandDispatcher>();
                    var logger = provider.GetService<ILogger<DefaultJobProcessor>>();
                    var completionService = provider.GetService<ICommandCompletionService>();

                    return new DefaultJobProcessor(
                        provider,
                        dispatcher, 
                        errorCode: errorCode,
                        completionService: completionService,
                        onError: interOnError, 
                        preeDispatch: preeProcessCommand,
                        logger: logger
                    );
                })
                .AddSingleton<ICommandBus>(provider =>
                {
                    var client = provider.GetService<IBackgroundJobClient>();
                    var logger = provider.GetService<ILogger<HangfireCommandBus>>();

                    Func<ICommand, Task> innerPreeSendCommand = null;
                    if (preeSendCommand != null)
                        innerPreeSendCommand = command => preeSendCommand(provider, command);

                    return new HangfireCommandBus(client, innerPreeSendCommand, logger);
                });

            return services;
        }
    }
}
