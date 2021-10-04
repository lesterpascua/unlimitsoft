using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
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
            DbContext = dbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        protected DbContext DbContext { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<TEntity> FindAll() => DbContext.Set<TEntity>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async ValueTask<TEntity> FindAsync(object[] keyValues, CancellationToken ct) => await DbContext.Set<TEntity>().FindAsync(keyValues, ct);
    }
}
