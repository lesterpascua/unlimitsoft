using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Base class for all query.
    /// </summary>
    public abstract class Query<TResult, TProps> : IQuery<TResult>
    {
        /// <summary>
        /// Get or set metadata props associate with the query.
        /// </summary>
        public TProps QueryProps { get; protected set; }

        /// <summary>
        /// Auto execute query.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public Task<(QueryResponse, TResult)> ExecuteAsync(IQueryDispatcher dispatcher) 
            => dispatcher.DispatchAsync<TResult>(this).ContinueWith(c => (c.Result, c.Result.IsSuccess ? c.Result.GetBody<TResult>() : default));

        /// <summary>
        /// Return metadata props associate with the query.
        /// </summary>
        /// <typeparam name="TInnerProps"></typeparam>
        /// <returns></returns>
        TInnerProps IQuery.GetProps<TInnerProps>() => this.QueryProps as TInnerProps;
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
            QueryProps = props;
        }
    }
}
