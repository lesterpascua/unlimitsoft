using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected override JsonVersionedEventPayload Create(IVersionedEvent @event) => new JsonVersionedEventPayload(@event);
    }
}
