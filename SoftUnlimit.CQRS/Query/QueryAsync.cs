using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Base class for all query.
    /// </summary>
    public abstract class QueryAsync<T> : IQueryAsync
        where T : QueryProps
    {
        /// <summary>
        /// Get or set metadata props associate with the command.
        /// </summary>
        public T QueryProps { get; protected set; }

        /// <summary>
        /// Return metadata props associate with the query.
        /// </summary>
        /// <typeparam name="TProps"></typeparam>
        /// <returns></returns>
        TProps IQueryAsync.GetProps<TProps>() => this.QueryProps as TProps;
    }
}
