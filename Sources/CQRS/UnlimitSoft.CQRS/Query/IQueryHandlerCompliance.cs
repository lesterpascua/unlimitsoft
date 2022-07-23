using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Query
{
    /// <summary>
    /// Indicate the command handler allow compliance him self
    /// </summary>
    public interface IQueryHandlerCompliance<TQuery> : IQueryHandler
        where TQuery: IQuery
    {
        /// <summary>
        /// Evaluate compliance.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="ct"></param>
        ValueTask<IQueryResponse> ComplianceAsync(TQuery query, CancellationToken ct = default);
    }
}
