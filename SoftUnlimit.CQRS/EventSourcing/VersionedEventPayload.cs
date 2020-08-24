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

            CreatorType = @event.Creator?.GetType().AssemblyQualifiedName;
            if (@event.Created != null)
                Creator = JsonConvert.SerializeObject(@event.Creator, VersionedEventSettings.JsonSerializerSettings);
            if (@event.PrevState != null)
                PrevState = JsonConvert.SerializeObject(@event.PrevState, VersionedEventSettings.JsonSerializerSettings);
            if (@event.CurrState != null)
                CurrState = JsonConvert.SerializeObject(@event.CurrState, VersionedEventSettings.JsonSerializerSettings);
            if (@event.Body != null)
                Body = JsonConvert.SerializeObject(@event.Body, VersionedEventSettings.JsonSerializerSettings);
        }

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
        public uint ServiceID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ushort WorkerID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// Event Type.
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
        /// Command serialized as Json
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// Command type
        /// </summary>
        public string CreatorType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PrevState { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CurrState { get; set; }
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
