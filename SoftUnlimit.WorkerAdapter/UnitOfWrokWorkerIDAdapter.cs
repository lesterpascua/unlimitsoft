using Microsoft.Extensions.HealthChecks;
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

        private static ConcurrentDictionary<uint, SemaphoreSlim> _assigns;


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
        public IQueryable<TStorageObject> ToQuery(Expression<Func<TStorageObject, bool>> predicate)
        {
            var query = _repository.FindAll();
            if (predicate != null)
                query = query.Where(predicate);

            return query;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IAdapterInfoStorageObject> GetEnumerator() => _repository.FindAll().GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public async Task<string> ReleaseAsync(uint service, ushort worker)
        {
            var semaphore = _assigns[service];
            await semaphore.WaitAsync();
            try
            {
                var dbInfo = _repository
                    .Find(p => p.WorkerID == worker)
                    .FirstOrDefault();
                if (dbInfo == null || _repository.Remove(dbInfo) != EntityState.Deleted)
                    return null;

                await _unitOfWork.SaveChangesAsync();

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
            var semaphore = _assigns[service];
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
            var semaphore = _assigns.GetOrAdd(serviceID, ServiceBucketFactory);
            await semaphore.WaitAsync();
            try
            {
                return await Reserve(serviceID, identifier, endpoint);
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
                ConcurrentDictionary<uint, SemaphoreSlim> assings = new ConcurrentDictionary<uint, SemaphoreSlim>();
                foreach (var adapterInfo in repository.FindAll())
                    assings.GetOrAdd(adapterInfo.ServiceID, ServiceBucketFactory);

                Interlocked.CompareExchange(ref _assigns, assings, null);
            }
        }

        #region Private Method

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IQueryable<IAdapterInfoStorageObject> IWorkerIDAdapter.ToQuery(Expression<Func<IAdapterInfoStorageObject, bool>> predicate) => ToQuery(predicate as Expression<Func<TStorageObject, bool>>);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceID"></param>
        /// <param name="identifier"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private async Task<RegisterResult> Reserve(uint serviceID, string identifier, string endpoint)
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

            return new RegisterResult(workerID, false);
        }

        /// <summary>
        /// Create new service bucket
        /// </summary>
        /// <param name="serviceID"></param>
        /// <returns></returns>
        private static SemaphoreSlim ServiceBucketFactory(uint serviceID) => new SemaphoreSlim(1, 1);

        #endregion
    }
}
