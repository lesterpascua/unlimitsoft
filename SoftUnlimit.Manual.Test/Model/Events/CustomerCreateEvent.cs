using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Model.Events
{
    public class CustomerCreateEvent : VersionedEvent<Guid>
    {
        public CustomerCreateEvent(long entityID, Guid sourceID, long version, ICommand cmd, Customer prevCustomer, Customer currCustomer, object body)
            : base(entityID, sourceID, version, false, cmd, prevCustomer, currCustomer, body)
        {
        }
    }
}
