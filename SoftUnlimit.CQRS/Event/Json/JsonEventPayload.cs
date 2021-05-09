using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Map;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event.Json
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
            Payload = JsonEventUtility.Serializer(@event);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public EventPayload<string> Transform<TDestination>(IMapper mapper, IEventNameResolver resolver) where TDestination : class
        {
            var eventType = resolver.Resolver(EventName);

            var @event = JsonEventUtility.Deserializer(Payload, eventType);
            var transformedEvent = @event.Transform<TDestination>(mapper);

            return new JsonEventPayload(transformedEvent);
        }
    }
}
