using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Indicate the command handler allow validate him self
    /// </summary>
    public interface IQueryHandlerValidator<TQuery> : IQueryHandler
        where TQuery : IQuery
    {
        /// <summary>
        /// Build fluen validation rules.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="validator"></param>
        /// <param name="ct"></param>
        ValueTask<IQueryResponse> ValidatorAsync(TQuery query, QueryValidator<TQuery> validator, CancellationToken ct = default);
    }
}
