﻿using UnlimitSoft.Event;
using UnlimitSoft.Web.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.EventSourcing;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Used as proxy to store versioned event generated by entities just before complete transaction
/// </summary>
public interface IMediatorDispatchEvent
{
    /// <summary>
    /// Send event to save. This operation should be perform transactional.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="forceSave">If false event is wait for save in cache, else force to save event before to leave this implementation.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task DispatchEventsAsync(IEnumerable<IEvent> events, bool forceSave, CancellationToken ct = default);
    /// <summary>
    /// When all event are saved invoqued this method.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask EventsDispatchedAsync(IEnumerable<IEvent> events, CancellationToken ct = default);
}
/// <summary>
/// 
/// </summary>
public abstract class MediatorDispatchEvent<TEventPayload, TPayload> : IMediatorDispatchEvent
    where TEventPayload : EventPayload<TPayload>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="directlyDispatchNotDomainEvents">If true try to dispatch not domain event directly to find if exist any procesor for it.</param>
    public MediatorDispatchEvent(IServiceProvider provider, bool directlyDispatchNotDomainEvents = true)
    {
        Provider = provider;
        DirectlyDispatchNotDomainEvents = directlyDispatchNotDomainEvents;
    }

    /// <summary>
    /// If true try to dispatch not domain event directly to find if exist any procesor for it.
    /// </summary>
    protected bool DirectlyDispatchNotDomainEvents { get; }
    /// <summary>
    /// 
    /// </summary>
    protected IServiceProvider Provider { get; }
    /// <summary>
    /// 
    /// </summary>
    protected abstract IEventDispatcher? EventDispatcher { get; }
    /// <summary>
    /// 
    /// </summary>
    protected abstract IEventPublishWorker? EventPublishWorker { get; }

    /// <summary>
    /// 
    /// </summary>
    protected abstract IEventSourcedRepository<TEventPayload, TPayload>? EventSourcedRepository { get; }


    /// <summary>
    /// Create new event using versioned event as template.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    protected abstract TEventPayload Create(IEvent @event);

    /// <inheritdoc />
    public virtual async Task DispatchEventsAsync(IEnumerable<IEvent> events, bool forceSave, CancellationToken ct)
    {
        var eventsPayload = new List<TEventPayload>();
        foreach (var @event in events)
        {
            var payload = Create(@event);
            eventsPayload.Add(payload);

            var dispatcher = EventDispatcher;
            if (dispatcher is not null && (@event.IsDomainEvent || DirectlyDispatchNotDomainEvents))
            {
                var (response, error) = await dispatcher.DispatchAsync(Provider, @event, ct);
                if (response?.IsSuccess == false || error is not null)
                {
                    var ex = response?.GetBody<Exception>();
                    if (ex is null)
                        throw new InvalidOperationException("Error when executed events");
                    throw new AggregateException("Error when executed events", ex);
                }
            }
        }
        var eventSourcedRepository = EventSourcedRepository;
        if (eventSourcedRepository is not null)
            await eventSourcedRepository.CreateAsync(eventsPayload, forceSave, ct);
    }
    /// <summary>
    /// Indicated all event already dispatchers.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public virtual async ValueTask EventsDispatchedAsync(IEnumerable<IEvent> events, CancellationToken ct)
    {
        var eventSourcedRepository = EventSourcedRepository;
        if (eventSourcedRepository is not null)
            await eventSourcedRepository.SavePendingCangesAsync(ct);

        var publish = EventPublishWorker;
        if (publish is not null)
            await publish.PublishAsync(events, ct);
    }
}
