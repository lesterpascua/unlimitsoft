using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.Web.Event;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SoftUnlimit.CQRS.EventSourcing
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
        /// 
        /// </summary>
        void ClearVersionedEvents();
        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<IVersionedEvent> GetVersionedEvents();
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


        /// <summary>
        /// 
        /// </summary>
        protected EventSourced()
        {
            _version = -1;
            _versionedEvents = new List<IVersionedEvent>();
        }

        #region Public Properties

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        public long Version { get => _version; set => _version = value; }

        /// <summary>
        /// Remove all pending event asociate to the entity.
        /// </summary>
        public void ClearVersionedEvents() => _versionedEvents.Clear();
        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<IVersionedEvent> GetVersionedEvents() => _versionedEvents;

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
        /// Add event asociate with the command and adding to the event list. Every time added a new event the version of entity increment in one.
        /// </summary>
        /// <remarks>
        /// The command time needs has the attribute <see cref="MasterEventAttribute" />.
        /// </remarks>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="correlationId">Using to trace operations from source to destination.</param>
        /// <param name="command">Command from the event is originated.</param>
        /// <param name="currState">Actual entity snapshot.</param>
        /// <param name="isDomain"></param>
        /// <param name="prevState">Previous entity snapshot.</param>
        /// <param name="body"></param>
        protected IEnumerable<IVersionedEvent> AddMasterEvent(Guid eventId, ushort serviceId, string workerId, string correlationId, ICommand command, object prevState, object currState = null, bool isDomain = false, object body = null)
        {
            var events = AddMasterEvent(
                EventFactory,
                AddVersionedEvent,
                Id,
                ref _version,
                eventId,
                serviceId,
                workerId,
                correlationId,
                command,
                prevState,
                currState,
                isDomain,
                body
            );
            return events;
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

        #region Static Properties
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventFactory"></param>
        /// <param name="addVersionedEvent"></param>
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="eventId"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId">Worker identifier. Can't be null.</param>
        /// <param name="correlationId">Correlation identifier to trace the events.</param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="isDomain"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static IEnumerable<IVersionedEvent> AddMasterEvent(
            Func<Type, Guid, TKey, long, ushort, string, string, ICommand, object, object, bool, object, IVersionedEvent> eventFactory,
            Action<IVersionedEvent> addVersionedEvent,
            TKey sourceId,
            ref long version,
            Guid eventId,
            ushort serviceId,
            string workerId,
            string correlationId,
            ICommand command,
            object prevState = null,
            object currState = null,
            bool isDomain = false,
            object body = null
        )
        {
            var events = new List<IVersionedEvent>();
            var eventTypes = command.GetMasterEvent();
            if (eventTypes != null)
            {
                foreach (var eventType in eventTypes)
                {
                    var tmp = eventFactory(
                        eventType, 
                        eventId, 
                        sourceId, 
                        Interlocked.Increment(ref version), 
                        serviceId, 
                        workerId, 
                        correlationId, 
                        command, 
                        prevState, 
                        currState, 
                        isDomain, 
                        body
                    );
                    addVersionedEvent(tmp);

                    events.Add(tmp);
                }
            }
            return events;
        }
        #endregion
    }
}
