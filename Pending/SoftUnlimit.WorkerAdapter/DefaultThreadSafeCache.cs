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
                foreach (var serviceId in services)
                    _assigns.GetOrAdd(serviceId, Factory);
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
        public void Destroy(uint serviceId)
        {
            if (_assigns.Remove(serviceId, out SemaphoreSlim semaphore))
                semaphore.Dispose();
        }
        /// <inheritdoc />
        public SemaphoreSlim GetOrAdd(uint serviceId) => _assigns.GetOrAdd(serviceId, Factory);

        private static SemaphoreSlim Factory(uint serviceId) => new SemaphoreSlim(1, 1);
    }
}
