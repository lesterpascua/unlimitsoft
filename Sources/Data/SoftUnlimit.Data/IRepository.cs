using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> : IQueryRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Remove entity from repository.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        void Remove(TEntity entity);

        /// <summary>
        /// Update entity in repository.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        void Update(TEntity entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        void UpdateRange(params TEntity[] entities);

        /// <summary>
        /// Add entity to repository.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        void Add(TEntity entity);
        /// <summary>
        /// Add a collection of entity to repository
        /// </summary>
        /// <param name="entities"></param>
        void AddRange(params TEntity[] entities);
        /// <summary>
        /// Add entity to repository
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        ValueTask AddAsync(TEntity entity, CancellationToken ct = default);
        /// <summary>
        /// Add a collection of entity to repository
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);
    }
}
