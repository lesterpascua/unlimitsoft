using SoftUnlimit.Web.Event;
using System;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class JsonMediatorDispatchEventSourced : MediatorDispatchEventSourced<JsonVersionedEventPayload>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="unitOfWorkType"></param>
        /// <param name="directlyDispatchNotDomainEvents">If true not dispath domain event as optimized mechanims.</param>
        public JsonMediatorDispatchEventSourced(IServiceProvider provider, Type unitOfWorkType, bool directlyDispatchNotDomainEvents = false)
            : base(provider, unitOfWorkType, directlyDispatchNotDomainEvents)
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
