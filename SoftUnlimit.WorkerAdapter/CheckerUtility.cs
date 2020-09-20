using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// 
    /// </summary>
    public static class CheckerUtility
    {
        /// <summary>
        /// Healthy and Warning
        /// </summary>
        public static readonly CheckStatus[] Allowed = new CheckStatus[] { CheckStatus.Healthy, CheckStatus.Warning };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static IHealthCheckService Create(string endpoint)
        {
            HealthCheckBuilder builder = new HealthCheckBuilder()
                .AddUrlCheck(endpoint);
            return new HealthCheckService(builder, null, null);
        }
        /// <summary>
        /// Check all register adapters and remove all not satisfy tolerance rule.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="tolerance">Diferent betwen current time and last updated adapter time.</param>
        /// <param name="allowed">Status considerer aceptables. <see cref="Allowed"/></param>
        /// <returns></returns>
        public static async Task<AggregateException> CheckAsync(IWorkerIDAdapter adapter, TimeSpan tolerance, CheckStatus[] allowed)
        {
            var now = DateTime.UtcNow;
            var errors = new List<Exception>();
            List<(uint, ushort)> toDelete = new List<(uint, ushort)>();
            List<(uint, ushort)> toUpdate = new List<(uint, ushort)>();
            foreach (var info in adapter)
            { 
                var checker = Create(info.Endpoint);
                CompositeHealthCheckResult status;
                try
                {
                    status = await checker.CheckHealthAsync();
                } catch (Exception e)
                {
                    errors.Add(e);
                    status = new CompositeHealthCheckResult(CheckStatus.Unhealthy);
                }
                if (!allowed.Contains(status.CheckStatus))
                {
                    if (now - info.Updated > tolerance)
                        toDelete.Add((info.ServiceId, info.WorkerId));
                } else
                    toUpdate.Add((info.ServiceId, info.WorkerId));
            }

            foreach (var entry in toUpdate)
                await adapter.UpdateAsync(entry.Item1, entry.Item2);
            foreach (var entry in toDelete)
                await adapter.ReleaseAsync(entry.Item1, entry.Item2);

            if (errors.Any())
                return new AggregateException(errors.ToArray());
            return null;
        }
    }
}
