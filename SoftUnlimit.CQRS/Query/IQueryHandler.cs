using System;
using System.Collections.Generic;
using System.Text;
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
        /// <param name="args"></param>
        /// <returns></returns>
        Task<TResult> HandlerAsync(TQuery args);
    }
}
