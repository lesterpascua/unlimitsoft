using UnlimitSoft.Json;
using UnlimitSoft.Event;

namespace UnlimitSoft.CQRS.EventSourcing.Json;


/// <summary>
/// 
/// </summary>
public class JsonVersionedEventPayload : VersionedEventPayload<string>
{
    /// <summary>
    /// 
    /// </summary>
    public JsonVersionedEventPayload()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="serializer"></param>
    public JsonVersionedEventPayload(IVersionedEvent @event, IJsonSerializer serializer)
        : base(@event, serializer.Serialize(@event)!)
    {
    }
}
