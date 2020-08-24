using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Test.Model.Events
{
    public class CustomerChangedEvent : VersionedEvent<Guid>
    {
        public CustomerChangedEvent(Guid sourceID, long version, ICommand cmd, Customer prevCustomer, Customer currCustomer, object body)
            : base(sourceID, 1, 1, version, false, cmd, prevCustomer, currCustomer, body)
        {
        }
    }
}
