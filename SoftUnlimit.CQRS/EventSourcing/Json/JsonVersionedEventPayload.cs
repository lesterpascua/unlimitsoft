using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Map;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
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
            Payload = JsonEventUtility.Serializer(@event);
        }

        /// <inheritdoc />
        public override EventPayload<string> Transform(IEventNameResolver resolver, IMapper mapper, Type destination = null)
        {
            var eventType = resolver.Resolver(EventName);
            var (commandType, bodyType) = ResolveType(CommandType, BodyType);

            var versionedEvent = JsonEventUtility.Deserializer(Payload, eventType, commandType, bodyType);
            if (destination == null)
            {
                var versionedEventType = versionedEvent.GetType();
                if (!(versionedEventType.GetCustomAttribute(typeof(TransformTypeAttribute)) is TransformTypeAttribute attr))
                    return this;

                destination = attr.PublishType;
            }

            var transformedVersionedEvent = (IVersionedEvent)versionedEvent.Transform(mapper, destination);
            return new JsonVersionedEventPayload(transformedVersionedEvent);
        }
    }
}
