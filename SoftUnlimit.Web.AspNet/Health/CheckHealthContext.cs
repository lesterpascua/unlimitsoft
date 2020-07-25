using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SoftUnlimit.Web.AspNet.Health
{
    /// <summary>
    /// Info about current health status.
    /// </summary>
    public class CheckHealthContext
    {
        private int _healthy = (int)HealthStatus.Unhealthy;


        /// <summary>
        /// Get current health status.
        /// </summary>
        public HealthStatus Healthy => (HealthStatus)this._healthy;

        /// <summary>
        /// Update current health status
        /// </summary>
        /// <returns>Health status</returns>
        public HealthStatus Update(HealthStatus status) => (HealthStatus)Interlocked.Exchange(ref this._healthy, (int)status);
    }
}
