using Microsoft.Extensions.HealthChecks;
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
        /// 
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static async Task CheckAsync(IWorkerIDAdapter adapter, TimeSpan tolerance)
        {
            var now = DateTime.UtcNow;
            var infos = adapter.ToArray();
            List<(uint, ushort)> toDelete = new List<(uint, ushort)>();
            List<(uint, ushort)> toUpdate = new List<(uint, ushort)>();
            foreach (var info in infos)
            {
                var status = await info.Checker.CheckHealthAsync();
                if (status.CheckStatus != CheckStatus.Healthy)
                {
                    if (now - info.Updated > tolerance)
                        toDelete.Add((info.ServiceID, info.WorkerID));
                } else
                    toUpdate.Add((info.ServiceID, info.WorkerID));
            }

            foreach (var entry in toUpdate)
                await adapter.UpdateAsync(entry.Item1, entry.Item2);
            foreach (var entry in toDelete)
                await adapter.ReleaseAsync(entry.Item1, entry.Item2);
        }
    }
}
