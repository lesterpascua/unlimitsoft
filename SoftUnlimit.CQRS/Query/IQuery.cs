using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQuery
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
    public interface IQuery<TResult> : IQuery
    {
        /// <summary>
        /// Dispath query using it self as argument.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <returns></returns>
        Task<(QueryResponse, TResult)> ExecuteAsync(IQueryDispatcher dispatcher);
    }
}
