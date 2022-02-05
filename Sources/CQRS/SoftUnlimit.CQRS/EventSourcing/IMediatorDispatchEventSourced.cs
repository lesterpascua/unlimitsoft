﻿using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Event;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// Used as proxy to store versioned event generated by entities just before complete transaction
    /// </summary>
    public interface IMediatorDispatchEventSourced
    {
        /// <summary>
        /// Send event to save. This operation should be perform transactional.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="forceSave">If false event is wait for save in cache, else force to save event before to leave this implementation.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events, bool forceSave, CancellationToken ct = default);
        /// <summary>
        /// When all event are saved invoqued this method.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        ValueTask EventsDispatchedAsync(IEnumerable<IVersionedEvent> events, CancellationToken ct = default);
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class MediatorDispatchEventSourced<TVersionedEventPayload, TPayload> : IMediatorDispatchEventSourced
        where TVersionedEventPayload : VersionedEventPayload<TPayload>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="directlyDispatchNotDomainEvents">If true try to dispatch not domain event directly to find if exist any procesor for it.</param>
        public MediatorDispatchEventSourced(IServiceProvider provider, bool directlyDispatchNotDomainEvents = true)
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
        protected abstract IEventDispatcher EventDispatcher { get; }
        /// <summary>
        /// 
        /// </summary>
        protected abstract IEventPublishWorker EventPublishWorker { get; }

        /// <summary>
        /// 
        /// </summary>
        protected abstract IEventSourcedRepository<TVersionedEventPayload, TPayload> EventSourcedRepository { get; }


        /// <summary>
        /// Create new event using versioned event as template.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected abstract TVersionedEventPayload Create(IVersionedEvent @event);

        /// <inheritdoc />
        public virtual async Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events, bool forceSave, CancellationToken ct)
        {
            var eventsPayload = new List<TVersionedEventPayload>();
            foreach (var @event in events)
            {
                var payload = Create(@event);
                eventsPayload.Add(payload);

                var dispatcher = EventDispatcher;
                if (dispatcher is not null && (@event.IsDomainEvent || DirectlyDispatchNotDomainEvents))
                {
                    var response = await dispatcher.DispatchAsync(Provider, @event, ct);
                    if (response?.IsSuccess == false)
                        throw new AggregateException("Error when executed events", response.GetBody<Exception>());
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
        public virtual async ValueTask EventsDispatchedAsync(IEnumerable<IVersionedEvent> events, CancellationToken ct)
        {
            var eventSourcedRepository = EventSourcedRepository;
            if (eventSourcedRepository is not null)
                await eventSourcedRepository.SavePendingCangesAsync(ct);

            var publish = EventPublishWorker;
            if (publish is not null)
                await publish.PublishAsync(events, ct);
        }
    }
}
