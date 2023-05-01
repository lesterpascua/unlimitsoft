using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.EventSourced.CQRS.Event;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Repository;



public sealed class MyEventPayload : EventPayload
{
    public MyEventPayload()
    {
        Text = default!;
    }

    public MyEventPayload(IMyEvent @event, IJsonSerializer serializer) 
        : base(@event, serializer)
    {
        Text = @event.Text;
    }

    /// <summary>
    /// Some random text
    /// </summary>
    public string Text { get; set; }
}
