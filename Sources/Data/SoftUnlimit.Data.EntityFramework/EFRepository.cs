using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EFRepository<TEntity> : EFQueryRepository<TEntity>, IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public EFRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public void Remove(TEntity entity) => DbContext.Set<TEntity>().Remove(entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public void Update(TEntity entity) => DbContext.Set<TEntity>().Update(entity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public void UpdateRange(params TEntity[] entities) => DbContext.Set<TEntity>().UpdateRange(entities);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public void Add(TEntity entity) => DbContext.Set<TEntity>().Add(entity);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        public void AddRange(params TEntity[] entities) => DbContext.Set<TEntity>().AddRange(entities);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask AddAsync(TEntity entity, CancellationToken cancellationToken = default) => await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default) => await DbContext.Set<TEntity>().AddRangeAsync(entities, ct);
    }
}
