using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    public class VersionedEventPayload
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
        {
            this.EntityID = @event.EntityID;
            this.SourceID = @event.SourceID.ToString();
            this.Version = @event.Version;

            this.Created = @event.Created;
            this.EventType = @event.GetType().AssemblyQualifiedName;

            this.IsPubliched = false;
            this.IsDomainEvent = @event.IsDomainEvent;

            this.ActionType = @event.Command?.GetType().AssemblyQualifiedName;
            this.Body = JsonConvert.SerializeObject(@event.Body, VersionedEventSettings.JsonSerializerSettings);
            this.Action = JsonConvert.SerializeObject(@event.Command, VersionedEventSettings.JsonSerializerSettings);
            this.PrevSnapshot = JsonConvert.SerializeObject(@event.PrevState, VersionedEventSettings.JsonSerializerSettings);
            this.CurrSnapshot = JsonConvert.SerializeObject(@event.CurrState, VersionedEventSettings.JsonSerializerSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        public long EntityID { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string SourceID { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public long Version { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Created { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string EventType { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPubliched { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDomainEvent { get; protected set; }
        /// <summary>
        /// Command serialized as Json
        /// </summary>
        public string Action { get; protected set; }
        /// <summary>
        /// Command type
        /// </summary>
        public string ActionType { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string PrevSnapshot { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string CurrSnapshot { get; protected set; }
        /// <summary>
        /// Event extra information
        /// </summary>
        public string Body { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public void MarkEventAsPublished() => this.IsPubliched = true;
    }
}
