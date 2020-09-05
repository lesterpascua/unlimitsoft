using Microsoft.Extensions.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        /// Return queryable representation.
        /// </summary>
        /// <returns></returns>
        IQueryable<AdapterInfo> ToQuery(Expression predicate = null);

        /// <summary>
        /// Delete worker registration from the Adapter maitainer.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        Task<string> ReleaseAsync(uint service, ushort worker);
        /// <summary>
        /// Update the time of check service alive.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        Task<DateTime> UpdateAsync(uint service, ushort worker);
        /// <summary>
        /// Convert string identifier to 16 bit identifier.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        Task<RegisterResult> ReserveAsync(uint service, string identifier, string endpoint);
    }
}
