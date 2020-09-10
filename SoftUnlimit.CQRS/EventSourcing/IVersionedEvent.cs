﻿using Newtonsoft.Json;
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
        /// <param name="sourceID"></param>
        /// <param name="version"></param>
        /// <param name="serviceID"></param>
        /// <param name="workerID"></param>
        /// <param name="isDomainEvent"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="body"></param>
        protected VersionedEvent(TKey sourceID, long version, uint serviceID, ushort workerID, bool isDomainEvent, ICommand command, object prevState, object currState, object body = null)
            : base(sourceID, serviceID, workerID, command, prevState, currState, isDomainEvent, body)
        {
            this.Version = version;
            this.Created = DateTime.UtcNow;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; protected set; }
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
