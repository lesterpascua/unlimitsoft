using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Base class for all query.
    /// </summary>
    public abstract class QueryAsync<TResult> : IQueryAsync<TResult>
        where TResult : QueryProps
    {
        /// <summary>
        /// Get or set metadata props associate with the query.
        /// </summary>
        public TResult QueryProps { get; protected set; }

        /// <summary>
        /// Auto execute query.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public Task<TResult> ExecuteAsync(IQueryAsyncDispatcher dispatcher) => dispatcher.DispatchAsync<TResult>(this);

        /// <summary>
        /// Return metadata props associate with the query.
        /// </summary>
        /// <typeparam name="TProps"></typeparam>
        /// <returns></returns>
        TProps IQueryAsync.GetProps<TProps>() => this.QueryProps as TProps;
    }
}
