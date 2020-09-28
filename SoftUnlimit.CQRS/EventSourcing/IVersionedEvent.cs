using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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
    [Serializable]
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
        /// <param name="isDomainEvent"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="body"></param>
        protected VersionedEvent(Guid id, TKey sourceId, long version, uint serviceId, string workerId, bool isDomainEvent, ICommand command, object prevState, object currState, object body = null)
            : base(id, sourceId, serviceId, workerId, command, prevState, currState, isDomainEvent, body)
        {
            this.Version = version;
            this.Created = DateTime.UtcNow;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; protected set; }
    }
}
