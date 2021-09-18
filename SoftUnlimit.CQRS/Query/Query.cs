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
        /// Get or set metadata props associate with the command.
        /// </summary>
        public TProps Props { get; set; }

        /// <summary>
        /// Get or set metadata props associate with the query.
        /// </summary>
        [Obsolete("Use Props")]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TProps QueryProps => Props;

        /// <summary>
        /// Auto execute query.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        public async Task<(QueryResponse, TResult)> ExecuteAsync(IQueryDispatcher dispatcher)
        {
            var response = await dispatcher.DispatchAsync<TResult>(this);
            return (response, response.IsSuccess ? response.GetBody<TResult>() : default);
        }
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
            Props = props;
        }
    }
}
