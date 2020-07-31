using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryAsyncHandler
    {
    }
    /// <summary>
    /// Base generic interface for all QueryHandler
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="TQuery"></typeparam>
    public interface IQueryAsyncHandler<TResult, TQuery> : IQueryAsyncHandler
        where TQuery : IQueryAsync<TResult>
    {
        /// <summary>
        /// Handle query for specific type.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<TResult> HandlerAsync(TQuery args);
    }
}
