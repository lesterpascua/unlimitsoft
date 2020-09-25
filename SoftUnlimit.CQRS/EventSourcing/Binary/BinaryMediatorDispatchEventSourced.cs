using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing.Binary
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BinaryMediatorDispatchEventSourced : MediatorDispatchEventSourced<BinaryVersionedEventPayload>
    {
        /// <inheritdoc />
        public BinaryMediatorDispatchEventSourced(IServiceProvider provider, bool directlyDispatchNotDomainEvents = false)
            : base(provider, directlyDispatchNotDomainEvents)
        {
        }

        /// <summary>
        /// Create new event using versioned event as template.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        protected override BinaryVersionedEventPayload Create(IVersionedEvent @event) => new BinaryVersionedEventPayload(@event);
    }
}
