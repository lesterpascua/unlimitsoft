using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Event;
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
        /// <summary>
        /// 
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Command where event is originate.
        /// </summary>
        ICommand Command { get; }
        /// <summary>
        /// Previous snapshot in json representation.
        /// </summary>
        object PrevState { get; }
        /// <summary>
        /// Currenct snapshot in json representation
        /// </summary>
        object CurrState { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    public abstract class VersionedEvent<Key> : Event<Key>, IVersionedEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="sourceID"></param>
        /// <param name="version"></param>
        /// <param name="isDomainEvent"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="body"></param>
        protected VersionedEvent(long entityID, Key sourceID, long version, bool isDomainEvent, ICommand command, object prevState, object currState, object body = null)
            : base(entityID, sourceID, isDomainEvent, body)
        {
            this.Version = version;
            this.Created = DateTime.UtcNow;

            this.Command = command;
            this.PrevState = prevState;
            this.CurrState = currState;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Created { get; }
        /// <summary>
        /// 
        /// </summary>
        public ICommand Command { get; }
        /// <summary>
        /// Previous snapshot of entity.
        /// </summary>
        public object PrevState { get; }
        /// <summary>
        /// Currenct snapshot of entity.
        /// </summary>
        public object CurrState { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class VersionedEventSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public static JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings { 
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        /// <summary>
        /// 
        /// </summary>
        public static JsonSerializerSettings JsonDeserializerSettings { get; set; } = new JsonSerializerSettings { 
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor 
        };
    }
}
