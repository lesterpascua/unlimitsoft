﻿using Microsoft.Extensions.HealthChecks;
using SoftUnlimit.Data;
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
    public class UnitOfWrokWorkerIDAdapter<TUnitOfWork, TRepository, TStorageObject> : IWorkerIDAdapter
        where TUnitOfWork : IUnitOfWork
        where TRepository : IRepository<TStorageObject>
        where TStorageObject : class, IAdapterInfoStorageObject, new()
    {
        private readonly TUnitOfWork _unitOfWork;
        private readonly TRepository _repository;

        private static ConcurrentDictionary<uint, ServiceBucket> _assigns;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="repository"></param>
        public UnitOfWrokWorkerIDAdapter(TUnitOfWork unitOfWork, TRepository repository)
        {
            _unitOfWork = unitOfWork;
            _repository = repository;
        }

        /// <summary>
        /// Convert adapter to Query.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AdapterInfo> ToQuery(Expression<Func<TStorageObject, bool>> predicate)
        {
            var query = _repository.FindAll();
            if (predicate != null)
                query = query.Where(predicate);

            return query
                .AsEnumerable()
                .Select(s => AdapterInfo.FromAdapterInfoStorageObject(s, _assigns[s.ServiceID].HealthCheck[s.WorkerID]))
                .AsQueryable();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<AdapterInfo> GetEnumerator() 
            => _repository.FindAll().AsEnumerable().Select(s => AdapterInfo.FromAdapterInfoStorageObject(s, _assigns[s.ServiceID].HealthCheck[s.WorkerID])).GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async Task<string> ReleaseAsync(uint service, ushort worker)
        {
            (var semaphore, var healthCache) = _assigns[service];
            await semaphore.WaitAsync();
            try
            {
                var dbInfo = _repository
                    .Find(p => p.WorkerID == worker)
                    .FirstOrDefault();
                if (dbInfo == null || _repository.Remove(dbInfo) != EntityState.Deleted)
                    return null;

                await _unitOfWork.SaveChangesAsync();
                healthCache.Remove(worker);

                return dbInfo.Identifier;
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
            (var semaphore, var _) = _assigns[service];
            await semaphore.WaitAsync();
            try
            {
                var dbInfo = _repository
                    .Find(p => p.WorkerID == worker)
                    .FirstOrDefault();

                dbInfo.Updated = DateTime.UtcNow;
                await _repository.UpdateAsync(dbInfo);
                await _unitOfWork.SaveChangesAsync();

                return dbInfo.Updated;
            } finally
            {
                semaphore.Release();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceID"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public async Task<RegisterResult> ReserveAsync(uint serviceID, string identifier, string endpoint)
        {
            var bucket = _assigns.GetOrAdd(serviceID, ServiceBucketFactory);

            (var semaphore, var healthCache) = bucket;
            await semaphore.WaitAsync();
            try
            {
                return await this.Reserve(serviceID, identifier, endpoint, healthCache);
            } finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Load initial data using repository.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="force"></param>
        /// <returns></returns>
        public static void LoadFromRepository(IQueryRepository<TStorageObject> repository, bool force = false)
        {
            if (_assigns == null || force)
            {
                ConcurrentDictionary<uint, ServiceBucket> assings = new ConcurrentDictionary<uint, ServiceBucket>();
                foreach (var adapterInfo in repository.FindAll())
                {
                    var bucket = assings.GetOrAdd(adapterInfo.ServiceID, ServiceBucketFactory);
                    if (!bucket.HealthCheck.ContainsKey(adapterInfo.WorkerID))
                        bucket.HealthCheck.Add(adapterInfo.WorkerID, CheckerUtility.Create(adapterInfo.Endpoint));
                }
                Interlocked.CompareExchange(ref _assigns, assings, null);
            }
        }

        #region Private Method

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <summary>
        /// Query by filter expression.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IQueryable<AdapterInfo> IWorkerIDAdapter.ToQuery(Expression predicate) => ToQuery((Expression<Func<TStorageObject, bool>>)predicate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceID"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <param name="healthCache"></param>
        /// <returns></returns>
        private async Task<RegisterResult> Reserve(uint serviceID, string identifier, string endpoint, Dictionary<ushort, IHealthCheckService> healthCache)
        {
            var dbInfo = _repository
                .Find(p => p.Identifier == identifier)
                .FirstOrDefault();

            if (dbInfo != null)
                return new RegisterResult(dbInfo.WorkerID, true);

            var array = _repository.FindAll()
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
            dbInfo = new TStorageObject {
                Created = now,
                Endpoint = endpoint,
                Identifier = identifier,
                ServiceID = serviceID,
                Updated = now,
                WorkerID = workerID
            };
            await _repository.AddAsync(dbInfo);
            await _unitOfWork.SaveChangesAsync();
            
            healthCache.Add(workerID, CheckerUtility.Create(endpoint));
            return new RegisterResult(workerID, false);
        }

        /// <summary>
        /// Create new service bucket
        /// </summary>
        /// <param name="serviceID"></param>
        /// <returns></returns>
        private static ServiceBucket ServiceBucketFactory(uint serviceID) => new ServiceBucket(new SemaphoreSlim(1, 1));

        #endregion

        #region Nested Classes

        private sealed class ServiceBucket
        {
            public ServiceBucket(SemaphoreSlim semaphore)
            {
                Semaphore = semaphore;
                HealthCheck = new Dictionary<ushort, IHealthCheckService>();
            }

            /// <summary>
            /// 
            /// </summary>
            public SemaphoreSlim Semaphore { get; }
            /// <summary>
            /// 
            /// </summary>
            public Dictionary<ushort, IHealthCheckService> HealthCheck { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="semaphore"></param>
            /// <param name="healthCheck"></param>
            public void Deconstruct(out SemaphoreSlim semaphore, out Dictionary<ushort, IHealthCheckService> healthCheck)
            {
                semaphore = Semaphore;
                healthCheck = HealthCheck;
            }
        }

        #endregion
    }
}