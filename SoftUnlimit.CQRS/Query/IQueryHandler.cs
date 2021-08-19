using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryHandler
    {
    }
    /// <summary>
    /// Base generic interface for all QueryHandler
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TQuery"></typeparam>
    public interface IQueryHandler<TResult, TQuery> : IQueryHandler
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handle query for specific type.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete("Use HandlerAsync(TQuery query, CancellationToken ct = default) and call from this method")]
        Task<TResult> HandlerAsync(TQuery args);
        /// <summary>
        /// Handle query for specific type.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TResult> HandlerAsync(TQuery query, CancellationToken ct = default);
    }
}
