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
    public JsonEventPayload(IEvent @event)
        : base(@event, JsonUtility.Serialize(@event)!)
    {
    }
}
