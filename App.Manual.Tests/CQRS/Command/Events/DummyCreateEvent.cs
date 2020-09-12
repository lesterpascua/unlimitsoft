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
        public DummyCreateEvent(Guid sourceID, long version, uint serviceID, ushort workerID, ICommand command, object prevState, object currState, object body = null)
            : base(sourceID, version, serviceID, workerID, false, command, prevState, currState, body)
        {
        }
    }
}
