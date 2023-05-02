using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.WebApi.EventSourced.CQRS.Repository;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Event;


/// <summary>
/// 
/// </summary>
public sealed class MyMediatorDispatchEvent : MediatorDispatchEvent<MyEventPayload>
{
    public MyMediatorDispatchEvent(IServiceProvider provider) 
        : base(provider, true)
    {
    }

    /// <inheritdoc />
    protected override IEventRepository<MyEventPayload>? EventRepository => Provider.GetRequiredService<IMyEventRepository>();
    /// <inheritdoc />
    protected override MyEventPayload Create(IEvent @event) => new((IMyEvent)@event, Provider.GetRequiredService<IJsonSerializer>());
}
