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
        public TEntity Remove(TEntity entity)
        {
            DbContext.Set<TEntity>().Remove(entity);
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Update(TEntity entity)
        {
            DbContext.Set<TEntity>().Update(entity);
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public TEntity[] UpdateRange(params TEntity[] entities)
        {
            DbContext.Set<TEntity>().UpdateRange(entities);
            return entities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public TEntity Add(TEntity entity)
        {
            DbContext.Set<TEntity>().Add(entity);
            return entity;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        public TEntity[] AddRange(params TEntity[] entities)
        {
            DbContext.Set<TEntity>().AddRange(entities);
            return entities;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            return entity;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken ct = default)
        {
            await DbContext.Set<TEntity>().AddRangeAsync(entities, ct);
            return entities;
        }
    }
}
