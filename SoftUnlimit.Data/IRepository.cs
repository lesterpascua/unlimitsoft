using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
        EntityState Remove(TEntity entity);
        /// <summary>
        /// Update entity in repository.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        EntityState Update(TEntity entity);
        /// <summary>
        /// Update entity in repository async.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<EntityState> UpdateAsync(TEntity entity);
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
        EntityState Add(TEntity entity);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        void AddRange(params TEntity[] entities);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<EntityState> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    }
}
