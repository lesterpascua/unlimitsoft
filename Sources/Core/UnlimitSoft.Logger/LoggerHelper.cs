using Destructurama;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Generic;
using UnlimitSoft.Logger.Configuration;

namespace OneJN.Libs.AspNet.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class LoggerHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="loggerConfiguration"></param>
    /// <param name="serviceId"></param>
    /// <param name="environment"></param>
    public static void Configure(LoggerOption options, LoggerConfiguration loggerConfiguration, int? serviceId, string? environment = null)
    {
        loggerConfiguration.MinimumLevel.Is((Serilog.Events.LogEventLevel)options.Default);
        if (options.Override is not null)
            foreach (KeyValuePair<string, LogLevel> item in options.Override)
                loggerConfiguration.MinimumLevel.Override(item.Key, (Serilog.Events.LogEventLevel)item.Value);

        loggerConfiguration.Destructure.UsingAttributes();
        loggerConfiguration.Enrich.FromLogContext();
        loggerConfiguration.Enrich.WithMachineName();
        loggerConfiguration.Enrich.WithThreadId();
        loggerConfiguration.Enrich.WithAssemblyName();
        loggerConfiguration.Enrich.WithAssemblyVersion();

        if (serviceId is not null)
            loggerConfiguration.Enrich.WithProperty("ServiceId", serviceId);
        if (environment is not null)
            loggerConfiguration.Enrich.WithProperty("Environment", environment);
    }
}
