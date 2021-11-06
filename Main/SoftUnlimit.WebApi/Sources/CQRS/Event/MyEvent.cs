using SoftUnlimit.Event;
using System;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public class MyEvent : VersionedEvent<Guid, object>
    {
        public MyEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, object body = null) 
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
}
