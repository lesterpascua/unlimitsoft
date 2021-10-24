using Destructurama;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using SoftUnlimit.Logger.Configuration;
using SoftUnlimit.Logger.Enricher;
using System;

namespace SoftUnlimit.Logger.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <param name="environment"></param>
        /// <param name="compilation"></param>
        /// <param name="setup"></param>
        /// <returns></returns>
        public static IServiceCollection AddSofUnlimitLogger(this IServiceCollection services, LoggerOption config, string environment = null, string compilation = null, Action<LoggerConfiguration> setup = null)
        {
            if (Log.Logger != null)
                Log.CloseAndFlush();

            var loggerConfig = new LoggerConfiguration();

            loggerConfig.MinimumLevel.Is((Serilog.Events.LogEventLevel)config.Default);
            if (config.Override is not null)
                foreach (var entry in config.Override)
                    loggerConfig.MinimumLevel.Override(entry.Key, (Serilog.Events.LogEventLevel)entry.Value);

            loggerConfig.Destructure.UsingAttributes();
            loggerConfig.Enrich.FromLogContext();
            loggerConfig.Enrich.WithMachineName();
            loggerConfig.Enrich.WithThreadId();
            loggerConfig.Enrich.WithCorrelationIdContext();

            setup?.Invoke(loggerConfig);
            Log.Logger = loggerConfig.CreateLogger();

            //
            // Write start information
            if (!string.IsNullOrEmpty(environment) && !string.IsNullOrEmpty(compilation))
                Log.Information($"Starting, ENV: {environment}, COMPILER: {compilation} ...");


            services.AddScoped<ICorrelationContext, DefaultCorrelationContext>();
            services.TryAddSingleton<ICorrelationContextAccessor, DefaultCorrelationContextAccessor>();

            return services;
        }
    }
}
