﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Data;


/// <summary>
/// Provide an abstraction to access to the event source storage
/// </summary>
public interface IEventRepository<TEventPayload> where TEventPayload : EventPayload
{
    /// <summary>
    /// Serializer used for the internal functionalities
    /// </summary>
    IJsonSerializer Serializer { get; }

    /// <summary>
    /// Get all event non publish and allow paging
    /// </summary>
    /// <param name="paging"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<NonPublishEventPayload>> GetNonPublishedEventsAsync(Paging? paging = null, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task MarkEventsAsPublishedAsync(TEventPayload @event, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task MarkEventsAsPublishedAsync(IEnumerable<TEventPayload> events, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEventPayload?> GetEventAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<TEventPayload>> GetEventsAsync(Guid[] ids, CancellationToken ct = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paging"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<SourceIdWithVersion>> GetAllSourceIdAsync(Paging? paging = null, CancellationToken ct = default);

    /// <summary>
    /// Get the serialized event in some version
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="version">If is null get the last saved</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEventPayload?> GetAsync(Guid sourceId, long? version = null, CancellationToken ct = default);
    /// <summary>
    /// Get the serialized event in some date
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="dateTime">If is null get the last saved</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEventPayload?> GetAsync(Guid sourceId, DateTime? dateTime = null, CancellationToken ct = default);

    /// <summary>
    /// Get history of operation asociate to some entity
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="version"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<TEventPayload>> GetHistoryAsync(Guid sourceId, long version, CancellationToken ct = default);
    /// <summary>
    /// Get history of operation asociate to some entity
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="dateTime"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<List<TEventPayload>> GetHistoryAsync(Guid sourceId, DateTime dateTime, CancellationToken ct = default);

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
    ValueTask<TEventPayload> CreateAsync(TEventPayload eventPayload, bool forceSave = false, CancellationToken ct = default);
    /// <summary>
    /// Create multiples versioned event to the storage
    /// </summary>
    /// <param name="eventPayloads"></param>
    /// <param name="forceSave">Indicate the save should happend in this moment.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<IEnumerable<TEventPayload>> CreateAsync(IEnumerable<TEventPayload> eventPayloads, bool forceSave = false, CancellationToken ct = default);
}
/// <summary>
/// Extenssions method for the event repository
/// </summary>
public static class IEventRepositoryExtensions
{
    /// <summary>
    /// Add the instance to the event repository
    /// </summary>
    /// <remarks>
    /// Require the TEventPayload have a contructor where recieve (IEvent, IJsonSerializer)
    /// </remarks>
    /// <typeparam name="TEventPayload"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="this"></param>
    /// <param name="entity"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask AddOrUpdateAsync<TEventPayload, TEntity>(this IEventRepository<TEventPayload> @this, TEntity entity, CancellationToken ct = default)
        where TEventPayload : EventPayload
        where TEntity : class, IEventSourced
    {
        foreach (var @event in entity.GetEvents())
        {
            var payload = (TEventPayload?)Activator.CreateInstance(
                typeof(TEventPayload), 
                @event,
                @this.Serializer
            ) ?? throw new InvalidOperationException("Can't create a payload of this type, there no defined ctor(IEvent, IJsonSerializer)");
            await @this.CreateAsync(payload, false, ct);
        }
    }
}