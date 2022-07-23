using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQueryDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="args"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IQueryResponse> DispatchAsync<TResult>(IQuery args, CancellationToken ct = default);
    }
}
