using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Json;
using SoftUnlimit.Event;

namespace SoftUnlimit.CQRS.Event.Json
{
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
            : base(@event)
        {
            Payload = JsonUtility.Serialize(@event);
        }
    }
}
