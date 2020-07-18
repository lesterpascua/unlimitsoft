using Microsoft.Extensions.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// Represent a contract to convert worker identifier to numeric id
    /// </summary>
    public interface IWorkerIDAdapter : IEnumerable<AdapterInfo>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        Task<string> ReleaseAsync(int service, ushort worker);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        Task<DateTime> UpdateAsync(int service, ushort worker);
        /// <summary>
        /// Convert string identifier to 16 bit identifier.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task<RegisterResult> ReserveAsync(int service, string identifier, string endpoint);
    }
}
