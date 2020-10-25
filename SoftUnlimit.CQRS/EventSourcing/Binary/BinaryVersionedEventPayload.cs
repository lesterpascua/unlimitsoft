using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing.Binary
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BinaryEventPayload : EventPayload<byte[]>
    {
        /// <summary>
        /// 
        /// </summary>
        public BinaryEventPayload() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public BinaryEventPayload(IEvent @event)
            : base(@event)
        {
            Payload = BinaryEventUtility.Serializer(@event);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BinaryVersionedEventPayload : VersionedEventPayload<byte[]>
    {
        /// <summary>
        /// 
        /// </summary>
        public BinaryVersionedEventPayload()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public BinaryVersionedEventPayload(IVersionedEvent @event)
            : base(@event)
        {
            Payload = BinaryEventUtility.Serializer(@event);
        }

        /// <inheritdoc />
        public override EventPayload<byte[]> Transform(IEventNameResolver resolver, IMapper mapper, Type destination) => throw new NotImplementedException();
    }
}
