using SoftUnlimit.CQRS.EventSourcing.Json;
using System;
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
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TEntity> FindByIdAsync(string sourceId, long? version = null, CancellationToken ct = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<string[]> GetAllSourceIdAsync(CancellationToken ct = default);
        /// <summary>
        /// Get history of operation asociate to some entity.
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<JsonVersionedEventPayload[]> GetHistoryAsync<TPayload>(string sourceId, long version, CancellationToken ct = default);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        /// <param name="sourceId"></param>
        /// <param name="dateTime"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<JsonVersionedEventPayload[]> GetHistoryAsync<TPayload>(string sourceId, DateTime dateTime, CancellationToken ct = default);
    }
}