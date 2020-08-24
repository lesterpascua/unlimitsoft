using Newtonsoft.Json;
using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing.Json
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class JsonEventPayload
    {
        /// <summary>
        /// 
        /// </summary>
        public JsonEventPayload() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public JsonEventPayload(IEvent @event)
        {
            SourceID = @event.SourceID.ToString();
            ServiceID = @event.ServiceID;
            WorkerID = @event.WorkerID;
            
            IsDomainEvent = @event.IsDomainEvent;

            Created = @event.Created;
            Name = @event.Name;
            EventType = @event.GetType().AssemblyQualifiedName;

            IsPubliched = false;

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
        /// Indicate this event will process as domain allow optimization
        /// </summary>
        public bool IsDomainEvent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPubliched { get; set; }
        /// <summary>
        /// Event Type.
        /// </summary>
        public string EventType { get; set; }
        /// <summary>
        /// Event extra information
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void MarkEventAsPublished() => this.IsPubliched = true;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class JsonVersionedEventPayload : JsonEventPayload
    {
        /// <summary>
        /// 
        /// </summary>
        public JsonVersionedEventPayload()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public JsonVersionedEventPayload(IVersionedEvent @event)
            : base(@event)
        {
            Version = @event.Version;

            CreatorType = @event.Creator?.GetType().AssemblyQualifiedName;
            if (@event.Created != null)
                Creator = JsonConvert.SerializeObject(@event.Creator, VersionedEventSettings.JsonSerializerSettings);
            if (@event.PrevState != null)
                PrevState = JsonConvert.SerializeObject(@event.PrevState, VersionedEventSettings.JsonSerializerSettings);
            if (@event.CurrState != null)
                CurrState = JsonConvert.SerializeObject(@event.CurrState, VersionedEventSettings.JsonSerializerSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }
        
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
    }
}
