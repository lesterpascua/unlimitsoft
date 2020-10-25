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
        /// Transfort to type specified in <see cref="TransformTypeAttribute"/>
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public abstract VersionedEventPayload<TPayload> Transform(IMapper mapper, IEventNameResolver resolver);
        /// <summary>
        /// Transform event entity into other destination using a mapper interface.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="destination"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public abstract VersionedEventPayload<TPayload> Transform(IMapper mapper, Type destination, IEventNameResolver resolver);
        /// <summary>
        /// Transform event entity into other destination using a mapper interface.
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="mapper"></param>
        /// <param name="resolver"></param>
        /// <returns></returns>
        public abstract VersionedEventPayload<TPayload> Transform<TDestination>(IMapper mapper, IEventNameResolver resolver) where TDestination : class, IEntityInfo;
    }
}
