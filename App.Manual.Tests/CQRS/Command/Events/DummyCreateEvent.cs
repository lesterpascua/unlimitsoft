using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Manual.Tests.CQRS.Command.Events
{
    [Serializable]
    public class DummyCreateEvent : VersionedEvent<Guid>
    {
        public DummyCreateEvent(Guid sourceId, long version, uint serviceId, string workerId, ICommand command, object prevState, object currState, object body = null)
            : base(sourceId, version, serviceId, workerId, false, command, prevState, currState, body)
        {
        }
    }
}
