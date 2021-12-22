using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventSourcedRepository<TVersionedEventPayload, TPayload>
        where TVersionedEventPayload : VersionedEventPayload<TPayload>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<NonPublishVersionedEventPayload[]> GetNonPublishedEventsAsync(CancellationToken ct = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task MarkEventsAsPublishedAsync(TVersionedEventPayload @event, CancellationToken ct = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task MarkEventsAsPublishedAsync(IEnumerable<TVersionedEventPayload> events, CancellationToken ct = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TVersionedEventPayload> GetEventAsync(Guid id, CancellationToken ct = default);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TVersionedEventPayload[]> GetEventsAsync(Guid[] ids, CancellationToken ct = default);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<VersionedEntity[]> GetAllSourceIdAsync(Paging page = null, CancellationToken ct = default);

        /// <summary>
        /// Get the serialized event in some version
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="version">If is null get the last saved</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TVersionedEventPayload> GetAsync(string sourceId, long? version = null, CancellationToken ct = default);
        /// <summary>
        /// Get the serialized event in some date
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="dateTime">If is null get the last saved</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TVersionedEventPayload> GetAsync(string sourceId, DateTime? dateTime = null, CancellationToken ct = default);

        /// <summary>
        /// Get history of operation asociate to some entity
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TVersionedEventPayload[]> GetHistoryAsync(string sourceId, long version, CancellationToken ct = default);
        /// <summary>
        /// Get history of operation asociate to some entity
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="dateTime"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TVersionedEventPayload[]> GetHistoryAsync(string sourceId, DateTime dateTime, CancellationToken ct = default);

        /// <summary>
        /// Save all pending versiones events.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task SavePendingCangesAsync(CancellationToken ct = default);
        /// <summary>
        /// Create single versioned event in the storage
        /// </summary>
        /// <param name="eventPayload"></param>
        /// <param name="forceSave">Indicate the save should happend in this moment.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<JsonVersionedEventPayload> CreateAsync(JsonVersionedEventPayload eventPayload, bool forceSave = false, CancellationToken ct = default);
        /// <summary>
        /// Create multiples versioned event to the storage
        /// </summary>
        /// <param name="eventPayloads"></param>
        /// <param name="forceSave">Indicate the save should happend in this moment.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IEnumerable<TVersionedEventPayload>> CreateAsync(IEnumerable<TVersionedEventPayload> eventPayloads, bool forceSave = false, CancellationToken ct = default);
    }
}