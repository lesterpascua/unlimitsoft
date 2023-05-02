using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// Allow manipulate the pure EventSource implementation
/// </summary>
/// <remarks>
/// This implementation can change in the future for better optimization
/// </remarks>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TEventPayload"></typeparam>
public abstract class EFEventSourceDbUnitOfWork<TDbContext, TEventPayload> : EFMediatorDbUnitOfWork<TDbContext>
    where TDbContext : DbContext
    where TEventPayload : EventPayload
{
    /// <summary>
    /// 
    /// </summary>
    protected readonly IEventNameResolver _nameResolver;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="serializer"></param>
    /// <param name="nameResolver"></param>
    /// <param name="eventMediator"></param>
    protected EFEventSourceDbUnitOfWork(TDbContext dbContext, IJsonSerializer serializer, IEventNameResolver nameResolver, IMediatorDispatchEvent? eventMediator = null)
        : base(dbContext, eventMediator)
    {
        StopCreation = true;
        Serializer = serializer;
        _nameResolver = nameResolver;
    }

    /// <summary>
    /// 
    /// </summary>
    public IJsonSerializer Serializer { get; }

    /// <summary>
    /// Get all entities pending for update
    /// </summary>
    /// <returns></returns>
    protected override object[] GetPendingEntities()
    {
        var changes = DbContext.ChangeTracker
            .Entries()
            .Where(p => p.State != EntityState.Unchanged && p.Entity is EventPayload)
            .Select(s => (TEventPayload)s.Entity)
            .ToArray();
        return changes;
    }
    /// <summary>
    /// Load event from Payload
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    protected virtual IEvent? LoadFromPaylod(Type eventType, TEventPayload payload)
    {
        var bodyType = _nameResolver.GetBodyType(eventType);
        return EventPayload.FromEventPayload(eventType, bodyType, payload, Serializer);
    }
    /// <summary>
    /// Extract events from the entities pending for change
    /// </summary>
    /// <param name="changes"></param>
    /// <param name="events"></param>
    protected override void CollectEventsFromEntities(object[] changes, List<IEvent> events)
    {
        var span = ((TEventPayload[])changes).AsSpan();
        for (var i = 0; i < span.Length; i++)
        {
            var entity = span[i];
            if (entity is not TEventPayload eventPayload)
                continue;

            var eventType = _nameResolver.RequireResolver(eventPayload.Name);
            var @event = LoadFromPaylod(eventType, eventPayload) ?? throw new InvalidOperationException("Can't load event of this type");

            events.Add(@event);
        }
    }
}