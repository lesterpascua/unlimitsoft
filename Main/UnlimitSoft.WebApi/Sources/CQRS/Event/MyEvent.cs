using SoftUnlimit.Event;
using System;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public abstract class MyEvent<T> : VersionedEvent<Guid, T>
    {
        protected MyEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, T body) 
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }
}
