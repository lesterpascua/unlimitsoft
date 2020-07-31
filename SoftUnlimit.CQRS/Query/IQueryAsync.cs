using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryAsync
    {
        /// <summary>
        /// Get query property like user, etc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetProps<T>() where T : QueryProps;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IQueryAsync<TResult> : IQueryAsync
    {
        /// <summary>
        /// Dispath query using it self as argument.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        Task<TResult> ExecuteAsync(IQueryAsyncDispatcher dispatcher);
    }
}
