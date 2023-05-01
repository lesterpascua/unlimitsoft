using UnlimitSoft.Event;
using UnlimitSoft.WebApi.EventSourced.Client;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Event;


/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Amount"></param>
/// <param name="Created"></param>
public sealed record CreatedBody(Guid Id, string Name, int Amount, DateTime Created);
/// <summary>
/// Used to notified when an order is created.
/// </summary>
public sealed class CreatedEvent : MyEvent<CreatedBody>, IMementoEvent<IOrder>
{
    public CreatedEvent() : base() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sourceId"></param>
    /// <param name="version"></param>
    /// <param name="serviceId"></param>
    /// <param name="workerId"></param>
    /// <param name="correlationId"></param>
    /// <param name="body"></param>
    public CreatedEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, CreatedBody body)
        : base(id, sourceId, version, serviceId, workerId, correlationId, body)
    {
    }

    /// <inheritdoc />
    public void Apply(IOrder entity)
    {
        entity.Id = Body.Id;
        entity.Name = Body.Name;
        entity.Created = Body.Created;
        entity.Amount = Body.Amount;
        entity.Version = Version;
    }
}
