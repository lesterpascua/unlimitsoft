using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event.Json;


/// <summary>
/// 
/// </summary>
public sealed class JsonEventPayload : EventPayload<string>
{
    /// <summary>
    /// 
    /// </summary>
    public JsonEventPayload() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="serializer"></param>
    public JsonEventPayload(IEvent @event, IJsonSerializer serializer)
        : base(@event, serializer.Serialize(@event)!)
    {
    }
}
