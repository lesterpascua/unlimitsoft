using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    public static class IQueryRepositoryExtenssion
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="self"></param>
        /// <param name="predicate"></param>
        /// <param name="navPropertyPath"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> Find<TEntity>(this IQueryRepository<TEntity> self, Expression<Func<TEntity, bool>> predicate, params string[] navPropertyPath) where TEntity : class
        {
            var query = self.FindAll().Where(predicate);
            if (navPropertyPath != null)
                foreach (var navProperty in navPropertyPath)
                    query = query.Include(navProperty);

            return query;
        }
    }
}
