using SoftUnlimit.Cloud.Security;
using SoftUnlimit.CQRS.EventSourcing;
using System;

namespace SoftUnlimit.Cloud.Event
{
    public abstract class CloudEvent : VersionedEvent<Guid>
    {
        public CloudEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, object body = null)
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }

        /// <summary>
        /// Get identity identifier owner of the event.
        /// </summary>
        public Guid IdentityId { get; set; }
    }
    public abstract class CloudEvent<TBody> : CloudEvent
        where TBody : class
    {
        public CloudEvent(Guid id, Guid sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, TBody body = null) 
            : base(id, sourceId, version, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
        }
    }

    public static class CloudEventExtensions
    {
        /// <summary>
        /// Create identity from the service user and replace the Id and correlation with 
        /// the event user owner id. This is use to keep trace of the user start the operation.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="local"></param>
        /// <returns></returns>
        public static IdentityInfo GetIdentity(this CloudEvent @this, IdentityInfo local) => new(@this.IdentityId, local.Role, local.Scope, @this.CorrelationId, @this.CorrelationId);
    }
}
