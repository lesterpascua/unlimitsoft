using Microsoft.Extensions.HealthChecks;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WorkerAdapter
{
    /// <summary>
    /// Transform a worker string identifier into number identifier.
    /// </summary>
    public class MemoryWorkerIDAdapter : IWorkerIDAdapter
    {
        private readonly ConcurrentDictionary<uint, ServiceBucket> _assigns;


        /// <summary>
        /// 
        /// </summary>
        public MemoryWorkerIDAdapter()
        {
            this._assigns = new ConcurrentDictionary<uint, ServiceBucket>();
        }
        /// <summary>
        /// 
        /// </summary>
        public MemoryWorkerIDAdapter(IEnumerable<IAdapterInfoStorageObject> initInfos)
        {
            this._assigns = new ConcurrentDictionary<uint, ServiceBucket>();
            foreach (var info in initInfos)
            {
                (var semaphore, var cache, var reverseCache) = _assigns.GetOrAdd(info.ServiceId, (service) => new ServiceBucket {
                    Cache = new Dictionary<string, WorkerBucket>(),
                    ReverseCache = new Dictionary<ushort, string>(),
                    Semaphore = new SemaphoreSlim(1, 1)
                });

                cache.Add(info.Identifier, new WorkerBucket {
                    Endpoint = info.Endpoint,
                    WorkerId = info.WorkerId,
                    Created = info.Created,
                    Updated = info.Updated,
                    Identifier = info.Identifier,
                    ServiceId = info.ServiceId
                });
                reverseCache.Add(info.WorkerId, info.Identifier);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IAdapterInfoStorageObject> GetEnumerator()
        {
            foreach (var service in this._assigns)
                foreach (var worker in service.Value.Cache)
                    yield return worker.Value;
        }
        /// <summary>
        /// Convert adapter to Query.
        /// </summary>
        /// <returns></returns>
        public IQueryable<IAdapterInfoStorageObject> ToQuery(Expression<Func<IAdapterInfoStorageObject, bool>> predicate)
        {
            var query = this.AsQueryable();
            if (predicate != null)
                query = query.Where(predicate);
            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async Task<string> ReleaseAsync(uint service, ushort worker)
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
        public async Task<DateTime> UpdateAsync(uint service, ushort worker)
        {
            (var semaphore, var cache, var reverseCache) = _assigns[service];
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
        public async Task<RegisterResult> ReserveAsync(uint service, string identifier, string endpoint)
        {
            var bucket = _assigns.GetOrAdd(service, (service) => new ServiceBucket {
                Semaphore = new SemaphoreSlim(1, 1),
                Cache = new Dictionary<string, WorkerBucket>(),
                ReverseCache = new Dictionary<ushort, string>(),
            });

            (var semaphore, var cache, var reverseCache) = bucket;
            await semaphore.WaitAsync();
            try
            {
                return this.Reserve(identifier, cache, reverseCache, endpoint);
            } finally
            {
                semaphore.Release();
            }
        }

        #region Private Method

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        private RegisterResult Reserve(string identifier, Dictionary<string, WorkerBucket> cache, Dictionary<ushort, string> reverseCache, string endpoint)
        {
            if (cache.ContainsKey(identifier))
                return new RegisterResult(cache[identifier].WorkerId, true);

            var array = cache.Values
                .Select(s => s.WorkerId)
                .OrderBy(k => k)
                .ToArray();
            ushort workerId = (ushort)array.Length;
            for (ushort i = 0; i < array.Length; i++)
            {
                if (array[i] == i)
                    continue;
                workerId = i;
                break;
            }

            var now = DateTime.UtcNow;
            cache.Add(identifier, new WorkerBucket {
                Endpoint = endpoint,
                WorkerId = workerId,
                Created = now,
                Updated = now,
            });
            reverseCache.Add(workerId, identifier);

            return new RegisterResult(workerId, false);
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
        private sealed class WorkerBucket : IAdapterInfoStorageObject
        {
            /// <summary>
            /// 
            /// </summary>
            public uint ServiceId { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Identifier { get; set; }

            /// <summary>
            /// Health endpoint
            /// </summary>
            public string Endpoint { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public ushort WorkerId { get; set; }
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
