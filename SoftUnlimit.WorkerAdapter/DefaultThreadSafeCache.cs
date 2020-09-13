using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// 
    /// </summary>
    public class DefaultThreadSafeCache : IThreadSafeCache, IDisposable
    {
        private readonly ConcurrentDictionary<uint, SemaphoreSlim> _assigns;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services">Services already exist.</param>
        public DefaultThreadSafeCache(IEnumerable<uint> services = null)
        {
            _assigns = new ConcurrentDictionary<uint, SemaphoreSlim>();
            if (services != null)
                foreach (var serviceID in services)
                    _assigns.GetOrAdd(serviceID, Factory);
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var semaphore in _assigns)
                semaphore.Value.Dispose();
        }
        /// <inheritdoc />
        public void Destroy(uint serviceID)
        {
            if (_assigns.Remove(serviceID, out SemaphoreSlim semaphore))
                semaphore.Dispose();
        }
        /// <inheritdoc />
        public SemaphoreSlim GetOrAdd(uint serviceID) => _assigns.GetOrAdd(serviceID, Factory);

        private static SemaphoreSlim Factory(uint serviceID) => new SemaphoreSlim(1, 1);
    }
}
