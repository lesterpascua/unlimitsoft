using System;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public abstract class MyEvent<T> : Event<T>
{
    protected MyEvent(Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, bool isDomainEvent, T body) 
        : base(id, sourceId, version, serviceId, workerId, correlationId, isDomainEvent, body)
    {
    }
}
