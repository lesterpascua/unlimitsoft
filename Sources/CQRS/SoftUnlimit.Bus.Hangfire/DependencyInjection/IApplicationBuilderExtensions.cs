using Hangfire;
using Microsoft.AspNetCore.Builder;
using System;

namespace SoftUnlimit.Bus.Hangfire
{
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
}
