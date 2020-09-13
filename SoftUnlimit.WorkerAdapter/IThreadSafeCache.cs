using System;
using System.Threading;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// Create semaphore per service to warranty every worker operation for service is thread safe.
    /// </summary>
    public interface IThreadSafeCache
    {
        /// <summary>
        /// Remove semaphore for this service.
        /// </summary>
        /// <param name="serviceID"></param>
        void Destroy(uint serviceID);
        /// <summary>
        /// Get or add semaphore for service.
        /// </summary>
        /// <param name="serviceID"></param>
        /// <returns></returns>
        SemaphoreSlim GetOrAdd(uint serviceID);
    }
}
