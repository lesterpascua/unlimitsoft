using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Event;


public interface IMyEvent : IEvent
{
    /// <summary>
    /// 
    /// </summary>
    public string Text { get; set; }
}
public abstract class MyEvent<TBody> : Event<TBody>, IMyEvent
{
    protected MyEvent() { }
    protected MyEvent(Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, TBody body) :
        base(id, sourceId, version, serviceId, workerId, correlationId, false, body)
    {
    }

    /// <inheritdoc />
    public string Text { get; set; } = default!;
}
