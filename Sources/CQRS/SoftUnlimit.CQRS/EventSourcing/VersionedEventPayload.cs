using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.Map;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public abstract class VersionedEventPayload<TPayload> : EventPayload<TPayload>
    {
        /// <summary>
        /// 
        /// </summary>
        public VersionedEventPayload()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public VersionedEventPayload(IVersionedEvent @event)
            : base(@event)
        {
            Version = @event.Version;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }
    }
}
