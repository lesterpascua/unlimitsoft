using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryAsyncDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<TResult> DispatchAsync<TResult>(IQueryAsync args) where TResult : class;
    }
}
