using UnlimitSoft.Event;
using UnlimitSoft.WebApi.EventSourced.Client;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Event;


/// <summary>
/// 
/// </summary>
/// <param name="Amount"></param>
public sealed record AddAmountBody(int Amount);
/// <summary>
/// Used to notified when an order is created.
/// </summary>
public sealed class AddAmountEvent : MyEvent<AddAmountBody>, IMementoEvent<IOrder>
{
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
    public AddAmountEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, AddAmountBody body)
        : base(id, sourceId, version, serviceId, workerId, correlationId, body)
    {
    }

    /// <inheritdoc />
    public void Apply(IOrder entity)
    {
        entity.Amount += Body.Amount;
        entity.Version = Version;
    }
}
