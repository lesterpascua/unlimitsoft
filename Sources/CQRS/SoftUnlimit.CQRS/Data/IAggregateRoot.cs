using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SoftUnlimit.CQRS.Data
{
    /// <summary>
    /// Represents an identifiable entity root in the CQRS system.
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        /// <summary>
        /// Clear event 
        /// </summary>
        void ClearEvents();
        /// <summary>
        /// Get a agregate events.
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<IEvent> GetEvents();
    }
    /// <summary>
    /// Implement and aggregate Root
    /// </summary>
    public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    {
        private readonly List<IEvent> _events;


        /// <summary>
        /// 
        /// </summary>
        protected AggregateRoot()
        {
            this._events = new List<IEvent>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearEvents() => this._events.Clear();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IReadOnlyCollection<IEvent> GetEvents() => this._events;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        protected void AddEvent(IEvent @event) => this._events.Add(@event);
    }
}
