using Destructurama;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using UnlimitSoft.Logger.Configuration;
using System;

namespace UnlimitSoft.Logger.DependencyInjection;


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
    /// <param name="setup"></param>
    /// <returns></returns>
    [Obsolete("Use LoggerHelper.Configure")]
    public static IServiceCollection AddUnlimitSofLogger(this IServiceCollection services, LoggerOption config, string environment, Action<LoggerConfiguration>? setup = null)
    {
        if (Log.Logger != null)
            Log.CloseAndFlush();

        var loggerConfig = new LoggerConfiguration();

        loggerConfig.MinimumLevel.Is((Serilog.Events.LogEventLevel)(config?.Default ?? Microsoft.Extensions.Logging.LogLevel.Warning));
        if (config?.Override is not null)
            foreach (var entry in config.Override)
                loggerConfig.MinimumLevel.Override(entry.Key, (Serilog.Events.LogEventLevel)entry.Value);

        loggerConfig.Destructure.UsingAttributes();
        loggerConfig.Enrich.FromLogContext();
        loggerConfig.Enrich.WithMachineName();
        loggerConfig.Enrich.WithThreadId();
        loggerConfig.Enrich.WithAssemblyName();
        loggerConfig.Enrich.WithAssemblyVersion();

        loggerConfig.Enrich.WithProperty("Environment", environment);


        setup?.Invoke(loggerConfig);
        Log.Logger = loggerConfig.CreateLogger();

        return services;
    }
}
