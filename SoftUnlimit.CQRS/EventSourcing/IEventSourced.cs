using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.Data;
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
            get; protected set;
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
        /// <param name="creator"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="currState">Actual entity snapshot.</param>
        /// <param name="prevState">Previous entity snapshot.</param>
        /// <param name="body"></param>
        protected IVersionedEvent AddMasterEvent(uint serviceId, string workerId, ICommand creator, object prevState, object currState = null, object body = null)
        {
            IVersionedEvent @event = null;
            Type eventType = creator.GetMasterEvent();
            if (eventType != null)
            {
                @event = EventFactory(eventType, Id, ++Version, serviceId, workerId, creator, prevState, currState, body);
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
        /// <param name="key"></param>
        /// <param name="version"></param>
        /// <param name="creator"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="currState">Actual entity snapshot.</param>
        /// <param name="prevState">Previous entity snapshot.</param>
        /// <param name="body"></param>
        /// <returns></returns>
        protected virtual IVersionedEvent EventFactory(Type eventType, TKey key, long version, uint serviceId, string workerId, ICommand creator, object prevState, object currState, object body) => (IVersionedEvent)Activator.CreateInstance(eventType, key, version, serviceId, workerId, creator, prevState, currState, body);

        #endregion
    }
}
