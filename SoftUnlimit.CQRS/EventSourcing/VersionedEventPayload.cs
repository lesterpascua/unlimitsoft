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
    [Serializable]
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

        /// <summary>
        /// Transform event entity into other destination using a mapper interface.
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="mapper"></param>
        /// <param name="destination">If destination is null get destination from <see cref="TransformTypeAttribute" /> attribute.</param>
        /// <returns></returns>
        public abstract EventPayload<TPayload> Transform(IEventNameResolver resolver, IMapper mapper, Type destination = null);
        /// <summary>
        /// Transform event entity into other destination using a mapper interface.
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="resolver"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public EventPayload<TPayload> Transform<TDestination>(IEventNameResolver resolver, IMapper mapper) where TDestination : class, IEntityInfo => Transform(resolver, mapper, typeof(TDestination));
    }
}
