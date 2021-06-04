using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EFQueryRepository<TEntity> : IQueryRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public EFQueryRepository(DbContext dbContext)
        {
            this.DbContext = dbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        protected DbContext DbContext { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> FindAll() => this.DbContext.Set<TEntity>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public async ValueTask<TEntity> FindAsync(params object[] keyValues) => await DbContext.Set<TEntity>().FindAsync(keyValues);
    }
}
