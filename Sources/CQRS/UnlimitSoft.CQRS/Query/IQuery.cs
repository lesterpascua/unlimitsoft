using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Query
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
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<(IQueryResponse, TResult)> ExecuteAsync(IQueryDispatcher dispatcher, CancellationToken ct = default);
    }
}
