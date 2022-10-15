using Hangfire;
using Microsoft.AspNetCore.Builder;
using System;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="worker"></param>
    /// <param name="pollingInterval"></param>
    /// <returns></returns>
    [Obsolete("Please use IServiceCollection.AddHangfireServer extension method instead in the ConfigureServices method. Will be removed in 2.0.0.")]
    public static IApplicationBuilder UseCommandBusHangfireServer(this IApplicationBuilder app, int worker, TimeSpan pollingInterval)
    {
        app.UseHangfireServer(new BackgroundJobServerOptions
        {
            ServerName = Environment.MachineName,
            WorkerCount = worker,
            SchedulePollingInterval = pollingInterval,
        });
        return app;
    }
}
