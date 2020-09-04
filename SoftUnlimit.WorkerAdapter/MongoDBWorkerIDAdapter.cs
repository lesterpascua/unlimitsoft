using Microsoft.Extensions.HealthChecks;
using SoftUnlimit.Data;
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
    public class MongoDBWorkerIDAdapter : IWorkerIDAdapter
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<AdapterInfo> _repository;
        private readonly ConcurrentDictionary<Key, ServiceBucket> _assigns;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="repository"></param>
        public MongoDBWorkerIDAdapter(IUnitOfWork unitOfWork, IRepository<AdapterInfo> repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
            _assigns = new ConcurrentDictionary<Key, ServiceBucket>();
        }

        /// <summary>
        /// Convert adapter to Query.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AdapterInfo> ToQuery() => _repository.FindAll();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<AdapterInfo> GetEnumerator() => _repository.FindAll().GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public Task<string> ReleaseAsync(uint service, ushort worker)
        {

            throw new NotImplementedException();
            //(var semaphore, var cache, var reverseCache) = this._assigns[service];
            //await semaphore.WaitAsync();
            //try
            //{
            //    string identifier = reverseCache[worker];
            //    if (cache.Remove(identifier) && reverseCache.Remove(worker))
            //        return identifier;
            //    return null;
            //} finally
            //{
            //    semaphore.Release();
            //}
        }
        /// <summary>
        /// Update last check for service is alive.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public Task<DateTime> UpdateAsync(uint service, ushort worker)
        {
            //(var semaphore, var cache, var reverseCache) = _assigns[service];
            //await semaphore.WaitAsync();
            //try
            //{
            //    string identifier = reverseCache[worker];
            //    var aux = cache[identifier];
            //    return aux.Updated = DateTime.UtcNow;
            //} finally
            //{
            //    semaphore.Release();
            //}

            throw new NotImplementedException();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public Task<RegisterResult> ReserveAsync(uint service, string identifier, string endpoint)
        {
            //var bucket = _assigns.GetOrAdd(service, (service) => new ServiceBucket {
            //    Semaphore = new SemaphoreSlim(1, 1),
            //    HealthCheck = null
            //});

            //(var semaphore, var cache, var reverseCache) = bucket;
            //await semaphore.WaitAsync();
            //try
            //{
            //    return this.Reserve(identifier, cache, reverseCache, endpoint);
            //} finally {
            //    semaphore.Release();
            //}
            throw new NotImplementedException();
        }

        #region Private Method

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="cache"></param>
        /// <param name="reverseCache"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private RegisterResult Reserve(string identifier, string endpoint)
        {

            throw new NotImplementedException();

            //if (cache.ContainsKey(identifier))
            //    return new RegisterResult(cache[identifier].WorkerID, true);

            //var array = cache.Values
            //    .Select(s => s.WorkerID)
            //    .OrderBy(k => k)
            //    .ToArray();
            //ushort workerID = (ushort)array.Length;
            //for (ushort i = 0; i < array.Length; i++)
            //{
            //    if (array[i] == i)
            //        continue;
            //    workerID = i;
            //    break;
            //}

            //var now = DateTime.UtcNow;
            //cache.Add(identifier, new WorkerBucket {
            //    Checker = CheckerUtility.Create(endpoint),
            //    Endpoint = endpoint,
            //    WorkerID = workerID,
            //    Created = now,
            //    Updated = now,
            //});
            //reverseCache.Add(workerID, identifier);

            //return new RegisterResult(workerID, false);
        }


        #endregion

        #region Nested Classes

        private sealed class Key
        {
            public Key(uint serviceID, ushort workerID) => (ServiceID, WorkerID) = (serviceID, workerID);

            public uint ServiceID { get; set; }
            public ushort WorkerID { get; set; }

            public void Deconstruct(out uint serviceID, out ushort workerID)
            {
                workerID = WorkerID;
                serviceID = ServiceID;
            }
            public override bool Equals(object obj) => (obj is Key key) && Equals(key);
            public override int GetHashCode() => HashCode.Combine(ServiceID, WorkerID);
            public bool Equals(Key other) => ServiceID == other.ServiceID && WorkerID == other.WorkerID;
        }
        private sealed class ServiceBucket
        {
            /// <summary>
            /// 
            /// </summary>
            public SemaphoreSlim Semaphore { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public IHealthCheckService HealthCheck { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="semaphore"></param>
            /// <param name="healthCheck"></param>
            public void Deconstruct(out SemaphoreSlim semaphore, out IHealthCheckService healthCheck)
            {
                semaphore = Semaphore;
                healthCheck = HealthCheck;
            }
        }

        #endregion
    }
}
