using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Base interface for all QueryHandler
    /// </summary>
    public interface IQueryHandler
    {
        /// <summary>
        /// Specified if query is async or not.
        /// </summary>
        bool IsAsync { get; }
    }
    /// <summary>
    /// Base generic interface for all QueryHandler
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TQuery"></typeparam>
    public interface IQueryHandler<TEntity, TQuery> : IQueryHandler
    {
        /// <summary>
        /// Handle query for specific type.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        IQueryable<TEntity> Handler(TQuery query);
        /// <summary>
        /// Handle query for specific type.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> HandlerAsync(TQuery query);
    }
}
