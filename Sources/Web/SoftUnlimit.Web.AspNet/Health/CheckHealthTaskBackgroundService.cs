using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Health
{
    /// <summary>
    /// 
    /// </summary>
    public class CheckHealthTaskBackgroundService : BackgroundService
    {
        private readonly TimeSpan _time;
        private readonly HealthCheckService _service;
        private readonly CheckHealthContext _healthContext;
        private readonly ILogger<CheckHealthTaskBackgroundService> _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">Time between check.</param>
        /// <param name="service"></param>
        /// <param name="startupTaskContext"></param>
        /// <param name="logger"></param>
        public CheckHealthTaskBackgroundService(
            TimeSpan time,
            HealthCheckService service,
            CheckHealthContext startupTaskContext,
            ILogger<CheckHealthTaskBackgroundService> logger)
        {
            this._time = time;
            this._service = service;
            this._healthContext = startupTaskContext;
            this._logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await this._service
                        .CheckHealthAsync();
                    this._healthContext.Update(response.Status);
                } catch (Exception ex)
                {
                    this._logger.LogError(ex, "Error when checking healthy");
                }
                await Task.Delay(this._time, stoppingToken);
            }
        }
    }
}
