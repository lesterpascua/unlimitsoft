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
        /// <param name="query"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TResult> HandlerAsync(TQuery query, CancellationToken ct = default);
    }
}
