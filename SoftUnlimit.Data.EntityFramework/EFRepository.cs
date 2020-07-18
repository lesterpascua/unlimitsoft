using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public EntityState Remove(TEntity entity) => (EntityState)this.DbContext.Set<TEntity>().Remove(entity).State;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public void UpdateRange(params TEntity[] entities) => this.DbContext.Set<TEntity>().UpdateRange(entities);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityState Update(TEntity entity) => (EntityState)this.DbContext.Set<TEntity>().Update(entity).State;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        public void AddRange(params TEntity[] entities) => this.DbContext.Set<TEntity>().AddRange(entities);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityState Add(TEntity entity) => (EntityState)this.DbContext.Set<TEntity>().Add(entity).State;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) => this.DbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<EntityState> AddAsync(TEntity entity, CancellationToken cancellationToken = default) => (EntityState)(await this.DbContext.Set<TEntity>().AddAsync(entity, cancellationToken)).State;
    }
}
