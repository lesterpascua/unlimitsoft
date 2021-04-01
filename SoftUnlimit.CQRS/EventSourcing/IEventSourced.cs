using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

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
        private readonly List<IVersionedEvent> _versionedEvents;


        /// <summary>
        /// 
        /// </summary>
        protected EventSourced()
        {
            Version = -1;
            _versionedEvents = new List<IVersionedEvent>();
        }

        #region Public Properties

        /// <summary>
        /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
        /// </summary>
        public long Version
        {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearVersionedEvents() => _versionedEvents.Clear();
        /// <summary>
        /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<IVersionedEvent> GetVersionedEvents() => _versionedEvents;

        /// <summary>
        /// Add event asociate with the command and adding to the event list. Every time added a new event the version of entity increment in one.
        /// </summary>
        /// <param name="eventId">Event identifier.</param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="correlationId">Using to trace operations from source to destination.</param>
        /// <param name="command">Command from the event is originated.</param>
        /// <param name="currState">Actual entity snapshot.</param>
        /// <param name="isDomain"></param>
        /// <param name="prevState">Previous entity snapshot.</param>
        /// <param name="body"></param>
        protected IVersionedEvent AddMasterEvent(Guid eventId, uint serviceId, string workerId, string correlationId, ICommand command, object prevState, object currState = null, bool isDomain = false, object body = null)
        {
            IVersionedEvent @event = null;
            Type eventType = command.GetMasterEvent();
            if (eventType != null)
            {
                @event = EventFactory(eventType, eventId, Id, ++Version, serviceId, workerId, correlationId, command, prevState, currState, isDomain, body);
                AddVersionedEvent(@event);
            }
            return @event;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        protected void AddVersionedEvent(IVersionedEvent @event) => _versionedEvents.Add(@event);
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
        protected virtual IVersionedEvent EventFactory(Type eventType, Guid eventId, TKey sourceId, long version, uint serviceId, string workerId, string correlationId, ICommand creator, object prevState, object currState, bool isDomain, object body) => (IVersionedEvent)Activator.CreateInstance(eventType, eventId, sourceId, version, serviceId, workerId, correlationId, creator, prevState, currState, isDomain, body);

        #endregion
    }
}
