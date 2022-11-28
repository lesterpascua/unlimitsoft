using UnlimitSoft.Event;
using UnlimitSoft.Json;

namespace UnlimitSoft.CQRS.Event.Json;


/// <summary>
/// 
/// </summary>
public class JsonEventPayload : EventPayload<string>
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
