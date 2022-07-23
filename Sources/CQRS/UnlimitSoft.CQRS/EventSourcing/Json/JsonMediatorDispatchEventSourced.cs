using UnlimitSoft.Event;
using System;

namespace UnlimitSoft.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class JsonMediatorDispatchEventSourced : MediatorDispatchEventSourced<JsonVersionedEventPayload, string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="directlyDispatchNotDomainEvents">If true not dispath domain event as optimized mechanims.</param>
        public JsonMediatorDispatchEventSourced(IServiceProvider provider, bool directlyDispatchNotDomainEvents = false)
            : base(provider, directlyDispatchNotDomainEvents)
        {
        }

        /// <summary>
        /// Create new event using versioned event as template.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected override JsonVersionedEventPayload Create(IVersionedEvent @event) => new(@event);
    }
}
