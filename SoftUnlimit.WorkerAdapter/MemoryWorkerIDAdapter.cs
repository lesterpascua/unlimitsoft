using Microsoft.Extensions.HealthChecks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// Transform a worker string identifier into number identifier.
    /// </summary>
    public class MemoryWorkerIDAdapter : IWorkerIDAdapter
    {
        private readonly ConcurrentDictionary<int, ServiceBucket> _assigns;


        /// <summary>
        /// 
        /// </summary>
        public MemoryWorkerIDAdapter()
        {
            this._assigns = new ConcurrentDictionary<int, ServiceBucket>();
        }
        /// <summary>
        /// 
        /// </summary>
        public MemoryWorkerIDAdapter(IEnumerable<AdapterInfo> initInfos)
        {
            this._assigns = new ConcurrentDictionary<int, ServiceBucket>();
            foreach (var info in initInfos)
            {
                (var semaphore, var cache, var reverseCache) = this._assigns.GetOrAdd(info.Service, (service) => new ServiceBucket {
                    Cache = new Dictionary<string, WorkerBucket>(),
                    ReverseCache = new Dictionary<ushort, string>(),
                    Semaphore = new SemaphoreSlim(1, 1)
                });

                cache.Add(info.Identifier, new WorkerBucket {
                    Checker = CheckerUtility.Create(info.Endpoint),
                    Endpoint = info.Endpoint,
                    WorkerID = info.WorkerID,
                    Created = info.Created,
                    Updated = info.Updated
                });
                reverseCache.Add(info.WorkerID, info.Identifier);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<AdapterInfo> GetEnumerator()
        {
            foreach (var service in this._assigns)
                foreach (var worker in service.Value.Cache)
                {
                    yield return new AdapterInfo {
                        Checker = worker.Value.Checker,
                        Identifier = worker.Key,
                        Service = service.Key,
                        WorkerID = worker.Value.WorkerID,
                        Updated = worker.Value.Updated,
                        Created = worker.Value.Created,
                        Endpoint = worker.Value.Endpoint
                    };
                }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async Task<string> ReleaseAsync(int service, ushort worker)
        {
            (var semaphore, var cache, var reverseCache) = this._assigns[service];
            await semaphore.WaitAsync();
            try
            {
                string identifier = reverseCache[worker];
                if (cache.Remove(identifier) && reverseCache.Remove(worker))
                    return identifier;
                return null;
            } finally
            {
                semaphore.Release();
            }
        }
        /// <summary>
        /// Update last check for service is alive.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async Task<DateTime> UpdateAsync(int service, ushort worker)
        {
            (var semaphore, var cache, var reverseCache) = this._assigns[service];
            await semaphore.WaitAsync();
            try
            {
                string identifier = reverseCache[worker];
                var aux = cache[identifier];
                return aux.Updated = DateTime.UtcNow;
            } finally
            {
                semaphore.Release();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<RegisterResult> ReserveAsync(int service, string identifier, string endpoint)
        {
            var bucket = this._assigns.GetOrAdd(service, (service) => new ServiceBucket {
                Semaphore = new SemaphoreSlim(1, 1),
                Cache = new Dictionary<string, WorkerBucket>(),
                ReverseCache = new Dictionary<ushort, string>(),
            });

            (var semaphore, var cache, var reverseCache) = bucket;
            await semaphore.WaitAsync();
            try
            {
                return this.Reserve(identifier, cache, reverseCache, endpoint);
            } finally {
                semaphore.Release();
            }
        }

        #region Private Method

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="cache"></param>
        /// <param name="reverseCache"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private RegisterResult Reserve(string identifier, Dictionary<string, WorkerBucket> cache, Dictionary<ushort, string> reverseCache, string endpoint)
        {
            if (cache.ContainsKey(identifier))
                return new RegisterResult(cache[identifier].WorkerID, true);

            var array = cache.Values
                .Select(s => s.WorkerID)
                .OrderBy(k => k)
                .ToArray();
            ushort workerID = (ushort)array.Length;
            for (ushort i = 0; i < array.Length; i++)
            {
                if (array[i] == i)
                    continue;
                workerID = i;
                break;
            }

            var now = DateTime.UtcNow;
            cache.Add(identifier, new WorkerBucket {
                Checker = CheckerUtility.Create(endpoint),
                Endpoint = endpoint,
                WorkerID = workerID,
                Created = now,
                Updated = now,
            });
            reverseCache.Add(workerID, identifier);

            return new RegisterResult(workerID, false);
        }

        #endregion

        #region Nested Classes

        private sealed class ServiceBucket
        {
            /// <summary>
            /// 
            /// </summary>
            public SemaphoreSlim Semaphore { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Dictionary<string, WorkerBucket> Cache { get; set; }
            /// <summary>
            /// Store Reverse cache for identifier.
            /// </summary>
            public Dictionary<ushort, string> ReverseCache { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="semaphore"></param>
            /// <param name="cache"></param>
            /// <param name="reverseCache"></param>
            public void Deconstruct(out SemaphoreSlim semaphore, out Dictionary<string, WorkerBucket> cache, out Dictionary<ushort, string> reverseCache)
            {
                semaphore = this.Semaphore;
                cache = this.Cache;
                reverseCache = this.ReverseCache;
            }
        }
        private sealed class WorkerBucket
        {
            /// <summary>
            /// Health endpoint
            /// </summary>
            public string Endpoint { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public ushort WorkerID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public IHealthCheckService Checker { get; set; }
            /// <summary>
            /// Creation date
            /// </summary>
            public DateTime Created { get; set; }
            /// <summary>
            /// Last healty check.
            /// </summary>
            public DateTime Updated { get; set; }
        }

        #endregion
    }
}
