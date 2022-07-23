using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.Json;
using UnlimitSoft.Event;

namespace UnlimitSoft.CQRS.Event.Json
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
