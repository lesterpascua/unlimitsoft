using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace UnlimitSoft.CQRS.EventSourcing
{
    /// <summary>
    /// Represents an identifiable entity that is event sourced.
    /// </summary>
    public interface IEventSourced : IAggregateRoot
    {
        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        long Version { get; }

        /// <summary>
        /// Remove all pending event asociate to the entity.
        /// </summary>
        void ClearVersionedEvents();
        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<IVersionedEvent> GetVersionedEvents();

        /// <summary>
        /// Gets the collection of old events used to build the entity.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<IVersionedEvent> GetOldVersionedEvents();
    }
    /// <summary>
    /// Base class for event sourced entities that implements <see cref="IEventSourced"/>. 
    /// </summary>
    /// <remarks>
    /// <see cref="IEventSourced"/> entities do not require the use of <see cref="EventSourced{TKey}"/>, but this class contains some common 
    /// useful functionality related to versions and rehydration from past events.
    /// </remarks>
    public abstract class EventSourced<TKey> : AggregateRoot<TKey>, IEventSourced
    {
        private long _version;
        private readonly List<IVersionedEvent> _versionedEvents;
        private readonly IReadOnlyCollection<IVersionedEvent> _oldEvents;


        /// <summary>
        /// 
        /// </summary>
        protected EventSourced()
        {
            _version = -1;
            _versionedEvents = new List<IVersionedEvent>();
        }
        /// <summary>
        /// Create a Event Sourced initial amount of event
        /// </summary>
        /// <param name="oldEvents">List of event asociate to the entity at this moment.</param>
        protected EventSourced(IReadOnlyCollection<IVersionedEvent> oldEvents) 
            : this()
        {
            if (oldEvents?.Any() == true)
            {
                _oldEvents = oldEvents;
                _version = oldEvents.Max(p => p.Version);
            }
        }

        #region Public Properties

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        public long Version { get => _version; set => _version = value; }


        /// <inheritdoc />
        public void ClearVersionedEvents() => _versionedEvents.Clear();
        /// <inheritdoc />
        public IReadOnlyCollection<IVersionedEvent> GetVersionedEvents() => _versionedEvents;

        /// <inheritdoc />
        public IReadOnlyCollection<IVersionedEvent> GetOldVersionedEvents() => _oldEvents;

        /// <summary>
        /// Attach the event to the entity
        /// </summary>
        /// <param name="event"></param>
        protected void AddVersionedEvent(IVersionedEvent @event) => _versionedEvents.Add(@event);
        /// <summary>
        /// Create event of the type specified and attach the event to the entity
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventId"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId">Worker identifier. Can't be null.</param>
        /// <param name="correlationId">Correlation identifier to trace the events.</param>
        /// <param name="body"></param>
        protected IVersionedEvent AddVersionedEvent(Type eventType, Guid eventId, ushort serviceId, string workerId, string correlationId, object body)
        {
            var tmp = EventFactory(
                eventType,
                eventId,
                Id,
                Interlocked.Increment(ref _version),
                serviceId,
                workerId,
                correlationId,
                null,
                null,
                null,
                false,
                body
            );
            AddVersionedEvent(tmp);
            return tmp;
        }
        /// <summary>
        /// Create versioned event factory
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventId"></param>
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="creator"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="correlationId"></param>
        /// <param name="currState">Actual entity snapshot.</param>
        /// <param name="isDomain"></param>
        /// <param name="prevState">Previous entity snapshot.</param>
        /// <param name="body"></param>
        /// <returns></returns>
        protected virtual IVersionedEvent EventFactory(Type eventType, Guid eventId, TKey sourceId, long version, ushort serviceId, string workerId, string correlationId, ICommand creator, object prevState, object currState, bool isDomain, object body) => (IVersionedEvent)Activator.CreateInstance(eventType, eventId, sourceId, version, serviceId, workerId, correlationId, creator, prevState, currState, isDomain, body);

        #endregion
    }
}
