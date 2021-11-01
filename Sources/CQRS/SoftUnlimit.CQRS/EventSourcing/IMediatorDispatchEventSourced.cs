﻿using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using SoftUnlimit.Web.Client;
using SoftUnlimit.Event;
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
    public abstract class MediatorDispatchEventSourced<TPayload> : IMediatorDispatchEventSourced
        where TPayload : class
    {
        private readonly Type _unitOfWorkType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="unitOfWorkType"></param>
        /// <param name="directlyDispatchNotDomainEvents">If true try to dispatch not domain event directly to find if exist any procesor for it.</param>
        public MediatorDispatchEventSourced(IServiceProvider provider, Type unitOfWorkType, bool directlyDispatchNotDomainEvents = true)
        {
            Provider = provider;
            _unitOfWorkType = unitOfWorkType;
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
        protected abstract IRepository<TPayload> VersionedEventRepository { get; }
        /// <summary>
        /// Get transactional unit of work.
        /// </summary>
        protected virtual IUnitOfWork UnitOfWork => Provider.GetService(_unitOfWorkType) as IUnitOfWork;


        /// <summary>
        /// Create new event using versioned event as template.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected abstract TPayload Create(IVersionedEvent @event);

        /// <inheritdoc />
        public virtual async Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events, bool forceSave, CancellationToken ct)
        {
            var remoteEvents = new List<TPayload>();
            foreach (var @event in events)
            {
                var payload = Create(@event);
                remoteEvents.Add(payload);

                var dispatcher = EventDispatcher;
                if (dispatcher != null && (@event.IsDomainEvent || DirectlyDispatchNotDomainEvents))
                {
                    var response = await dispatcher.DispatchAsync(Provider, @event, ct);
                    if (response?.IsSuccess == false)
                        throw new AggregateException("Error when executed events", response.GetBody<Exception>());
                }
            }
            var versionedEventRepository = VersionedEventRepository;
            if (versionedEventRepository != null)
            {
                await versionedEventRepository.AddRangeAsync(remoteEvents, ct);
                if (forceSave)
                    await UnitOfWork.SaveChangesAsync(ct);
            }
        }
        /// <summary>
        /// Indicated all event already dispatchers.
        /// </summary>
        /// <param name="events"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public virtual ValueTask EventsDispatchedAsync(IEnumerable<IVersionedEvent> events, CancellationToken ct)
        {
            var publish = EventPublishWorker;
            if (publish != null)
                return publish.PublishAsync(events, ct);
            return ValueTask.CompletedTask;
        }
    }
}
