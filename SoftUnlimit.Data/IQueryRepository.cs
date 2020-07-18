using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data
{
    /// <summary>
    /// Repository used only for query operations.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IQueryRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IQueryable<TEntity> FindAll();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        ValueTask<TEntity> FindAsync(params object[] keyValues);
    }
    /// <summary>
    /// Extenssion method for IQueryRepository.
    /// </summary>
    public static class IQueryRepositoryExtenssion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> Find<TEntity>(this IQueryRepository<TEntity> self, Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return self.FindAll().Where(predicate);
        }
    }
}
