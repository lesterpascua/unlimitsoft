using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
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
            var commandType = @event.Creator.GetType();
            var option = CommandConverter.CreateOptions(commandType);

            Payload = JsonSerializer.Serialize(@event, option);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
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
            var commandType = @event.Creator.GetType();
            var option = CommandConverter.CreateOptions(commandType);

            Payload = JsonSerializer.Serialize(@event, option);
        }
    }
}
