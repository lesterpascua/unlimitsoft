using SoftUnlimit.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Base class for all query.
    /// </summary>
    public abstract class Query<TResult, TProps> : IQuery<TResult>
    {
        /// <summary>
        /// Get or set metadata props associate with the command.
        /// </summary>
        public TProps Props { get; set; }

        /// <summary>
        /// Return metadata props associate with the query.
        /// </summary>
        /// <typeparam name="TInnerProps"></typeparam>
        /// <returns></returns>
        TInnerProps IQuery.GetProps<TInnerProps>() => Props as TInnerProps;
        /// <summary>
        /// Auto execute query.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<QueryResponse<TResult>> ExecuteAsync(IQueryDispatcher dispatcher, CancellationToken ct = default)
        {
            var response = await dispatcher.DispatchAsync<TResult>(this, ct);
            return (QueryResponse<TResult>)response;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class SealedQueryAsync<TResult> : Query<TResult, QueryProps>
    {
        /// <summary>
        /// 
        /// </summary>
        public SealedQueryAsync() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        public SealedQueryAsync(QueryProps props)
        {
            Props = props;
        }
    }
}
