using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Memento
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IMemento<TEntity>
        where TEntity : class, IEventSourced, new()
    {
        /// <summary>
        /// Build entity in the moment of the version supplied
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="version">Version of the entity.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TEntity> FindByVersionAsync(string sourceId, long? version = null, CancellationToken ct = default);
        /// <summary>
        /// Build entity in the moment of the date supplied.
        /// </summary>
        /// <param name="sourceId"></param>
        /// <param name="dateTime">Date where we need to check the entity.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TEntity> FindByCreateAsync(string sourceId, DateTime? dateTime = null, CancellationToken ct = default);
    }
    /// <summary>
    /// Load the entity in some moment of the system history.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TVersionedEventPayload"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    public abstract class Memento<TEntity, TVersionedEventPayload, TPayload> : IMemento<TEntity>
        where TEntity : class, IEventSourced, new()
        where TVersionedEventPayload : VersionedEventPayload<TPayload>
    {
        private readonly bool _snapshot;
        private readonly IEventNameResolver _nameResolver;
        private readonly IEventSourcedRepository<TVersionedEventPayload, TPayload> _eventSourcedRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nameResolver"></param>
        /// <param name="eventSourcedRepository"></param>
        /// <param name="snapshot"></param>
        public Memento(IEventNameResolver nameResolver, IEventSourcedRepository<TVersionedEventPayload, TPayload> eventSourcedRepository, bool snapshot = false)
        {
            _snapshot = snapshot;
            _nameResolver = nameResolver;
            _eventSourcedRepository = eventSourcedRepository;
        }

        /// <inheritdoc />
        public async Task<TEntity> FindByVersionAsync(string sourceId, long? version = null, CancellationToken ct = default)
        {
            if (_snapshot)
            {
                var eventPayload = await _eventSourcedRepository.GetAsync(sourceId, version, ct);
                if (eventPayload is null)
                    return null;

                return LoadEntityFromSnapshot(eventPayload);
            }

            var eventsPayload = await _eventSourcedRepository.GetHistoryAsync(sourceId, version ?? long.MaxValue, ct);
            if (eventsPayload?.Any() != true)
                return null;
            return LoadEntityFromHistory(eventsPayload);
        }
        /// <inheritdoc />
        public async Task<TEntity> FindByCreateAsync(string sourceId, DateTime? dateTime = null, CancellationToken ct = default)
        {
            if (_snapshot)
            {
                var eventPayload = await _eventSourcedRepository.GetAsync(sourceId, dateTime, ct);
                if (eventPayload is null)
                    return null;

                return LoadEntityFromSnapshot(eventPayload);
            }


            var eventsPayload = await _eventSourcedRepository.GetHistoryAsync(sourceId, dateTime ?? DateTime.MaxValue, ct);
            if (eventsPayload?.Any() != true)
                return null;
            return LoadEntityFromHistory(eventsPayload);
        }

        /// <summary>
        /// Get IMementoEvent from the event payload
        /// </summary>
        /// <param name="type"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected abstract IMementoEvent<TEntity> FromEvent(Type type, TPayload payload);


        #region Nested Classes
        private TEntity LoadEntityFromSnapshot(TVersionedEventPayload eventPayload)
        {
            var eventType = _nameResolver.Resolver(eventPayload.EventName);
            var @event = FromEvent(eventType, eventPayload.Payload);

            return @event.GetEntity();
        }
        private TEntity LoadEntityFromHistory(TVersionedEventPayload[] eventsPayload)
        {
            var entity = new TEntity();
            foreach (var eventPayload in eventsPayload)
            {
                var eventType = _nameResolver.Resolver(eventPayload.EventName);
                var @event = FromEvent(eventType, eventPayload.Payload);

                @event.Apply(entity);
            }

            return entity;
        }
        #endregion
    }
}