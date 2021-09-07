using SoftUnlimit.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Query
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
        Task<QueryResponse> DispatchAsync<TResult>(IQuery args, CancellationToken ct = default);
    }
}
