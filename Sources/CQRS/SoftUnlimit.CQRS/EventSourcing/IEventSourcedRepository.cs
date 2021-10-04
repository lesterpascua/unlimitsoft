using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEventSourcedRepository<TEntity>
        where TEntity : class, IEventSourced
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="version"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TEntity> FindByIdAsync(string sourceID, long? version = null, CancellationToken ct = default);
    }
}
