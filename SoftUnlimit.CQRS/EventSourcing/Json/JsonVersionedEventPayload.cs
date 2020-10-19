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
        public override VersionedEventPayload<string> Transform(IMapper mapper, IEventNameResolver resolver)
        {
            var eventType = resolver.Resolver(EventName);
            var (commandType, entityType, bodyType) = ResolveType(CommandType, EntityType, BodyType);

            var versionedEvent = JsonEventUtility.Deserializer(Payload, eventType, commandType, entityType, bodyType);

            var versionedEventType = versionedEvent.GetType();
            if (!(versionedEventType.GetCustomAttribute(typeof(TransformTypeAttribute)) is TransformTypeAttribute attr))
                return this;

            var transformedVersionedEvent = (IVersionedEvent)versionedEvent.Transform(mapper, attr.PublishType);
            return new JsonVersionedEventPayload(transformedVersionedEvent);
        }
        /// <inheritdoc />
        public override VersionedEventPayload<string> Transform(IMapper mapper, Type destination, IEventNameResolver resolver)
        {
            var eventType = resolver.Resolver(EventName);
            var (commandType, entityType, bodyType) = ResolveType(CommandType, EntityType, BodyType);

            var versionedEvent = JsonEventUtility.Deserializer(Payload, eventType, commandType, entityType, bodyType);
            var transformedVersionedEvent = (IVersionedEvent)versionedEvent.Transform(mapper, destination);

            return new JsonVersionedEventPayload(transformedVersionedEvent);
        }
        /// <inheritdoc />
        public override VersionedEventPayload<string> Transform<TDestination>(IMapper mapper, IEventNameResolver resolver) => Transform(mapper, typeof(TDestination), resolver);
    }
}
