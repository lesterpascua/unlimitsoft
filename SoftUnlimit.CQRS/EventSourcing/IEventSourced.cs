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
    [Serializable]
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
        /// <param name="prevState"></param>
        /// <param name="body"></param>
        protected void AddMasterEvent(ICommand creator, object prevState = null, object body = null)
        {
            Type eventType = creator.GetMasterEvent();
            if (eventType != null)
            {
                IVersionedEvent @event = this.EventFactory(eventType, ID, ++Version, creator, prevState, body);
                this.AddVersionedEvent(@event);
            }
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
        /// <param name="prevState"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        protected virtual IVersionedEvent EventFactory(Type eventType, TKey key, long version, ICommand creator, object prevState, object body) => (IVersionedEvent)Activator.CreateInstance(eventType, this.ID, version, creator, prevState, this, body);

        #endregion
    }
}
