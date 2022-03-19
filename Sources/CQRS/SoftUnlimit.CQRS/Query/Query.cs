using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Web;
using SoftUnlimit.Web.Client;
using System.Collections.Generic;
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
            if (response.IsSuccess)
                return (QueryResponse<TResult>)response;

            throw new ResponseException(response.UIText, response.GetBody<IDictionary<string, string[]>>(), response.Code);
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
