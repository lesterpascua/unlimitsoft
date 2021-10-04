using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Map;
using SoftUnlimit.Web.Model;
using System;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// Represents an event message that belongs to an ordered event stream.
    /// </summary>
    public interface IVersionedEvent : IEvent
    {
        /// <summary>
        /// Gets the version or order of the event in the stream. Este valor lo asigna la entidad que lo genero y 
        /// es el que ella poseia en el instante en que fue generado el evento. 
        /// </summary>
        long Version { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class VersionedEvent<TKey> : Event<TKey>, IVersionedEvent
    {
        /// <summary>
        /// 
        /// </summary>
        protected VersionedEvent()
        { 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sourceId"></param>
        /// <param name="version"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="correlationId"></param>
        /// <param name="isDomainEvent"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="body"></param>
        protected VersionedEvent(Guid id, TKey sourceId, long version, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomainEvent, object body = null)
            : base(id, sourceId, serviceId, workerId, correlationId, command, prevState, currState, isDomainEvent, body)
        {
            Version = version;
            Created = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public long Version { get; set; }
    }
}
