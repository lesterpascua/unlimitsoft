using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
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
            SourceID = @event.SourceID.ToString();
            ServiceID = @event.ServiceID;
            WorkerID = @event.WorkerID;
            Version = @event.Version;

            Created = @event.Created;
            Name = @event.Name;
            EventType = @event.GetType().AssemblyQualifiedName;

            IsPubliched = false;
            IsDomainEvent = @event.IsDomainEvent;

            ActionType = @event.Creator?.GetType().AssemblyQualifiedName;
            Body = JsonConvert.SerializeObject(@event.Body, VersionedEventSettings.JsonSerializerSettings);
            Action = JsonConvert.SerializeObject(@event.Creator, VersionedEventSettings.JsonSerializerSettings);
            PrevSnapshot = JsonConvert.SerializeObject(@event.PrevState, VersionedEventSettings.JsonSerializerSettings);
            CurrSnapshot = JsonConvert.SerializeObject(@event.CurrState, VersionedEventSettings.JsonSerializerSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        public string SourceID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public uint ServiceID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ushort WorkerID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; }
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
