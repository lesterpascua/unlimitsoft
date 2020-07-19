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
        public long EntityID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SourceID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EventType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPubliched { get; set; }
        /// <summary>
        /// Indicate this event will process as domain allow optimization
        /// </summary>
        public bool IsDomainEvent { get; set; }
        /// <summary>
        /// Indicate this event will dispatcher for remote processing for all microservice.
        /// </summary>
        public bool IsRemoteEvent { get; }
        /// <summary>
        /// Command serialized as Json
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// Command type
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PrevSnapshot { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CurrSnapshot { get; set; }
        /// <summary>
        /// Event extra information
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void MarkEventAsPublished() => this.IsPubliched = true;
    }
}
