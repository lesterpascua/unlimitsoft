using SoftUnlimit.Json;
using SoftUnlimit.Web.Event;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
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
        public JsonVersionedEventPayload(IVersionedEvent @event)
            : base(@event)
        {
            Payload = JsonUtility.Serialize(@event);
        }
    }
}
