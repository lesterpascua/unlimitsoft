using AutoMapper;
using SoftUnlimit.CQRS.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Generic method over all Dao.
    /// </summary>
    public static class Dao
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="dispatcher"></param>
        /// <param name="mapper"></param>
        /// <param name="pagging"></param>
        /// <param name="ordered"></param>
        /// <param name="includes"></param>
        /// <param name="criterias"></param>
        /// <returns></returns>
        public static Task<(IEnumerable<TResult>, int)> SearchAsync<TResult, TEntity>(IQueryDispatcher dispatcher, IMapper mapper, IQuery query, PaggingSettings pagging, IEnumerable<ColumnName> ordered, string[] includes, params Expression<Func<TEntity, bool>>[] criterias)
            where TEntity : class
        {
            // Build include specification.
            ISpecification<TEntity> spec = new SimpleSpecification<TEntity>(null, includes);

            #region Build filter specification.
            List<ISpecification<TEntity>> filterSpec = new List<ISpecification<TEntity>>(criterias?.Length ?? 0);

            foreach (var criteria in criterias)
                if (criteria != null)
                    filterSpec.Add(new SimpleSpecification<TEntity>(criteria));

            if (filterSpec.Any())
                spec = new AndSpecification<TEntity>(spec, new AndSpecification<TEntity>(filterSpec));
            #endregion

            // Build pagging specification.
            SearchSpecification<TEntity> searchSpec = new SearchSpecification<TEntity>(spec, pagging, ordered?.ToArray());

            return dispatcher
                .DispatchAsync<TEntity, TResult>(mapper, query, searchSpec)
                .ContinueWith(c => (c.Result, searchSpec.Total));
        }
    }
}
