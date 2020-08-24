using Newtonsoft.Json;
using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SoftUnlimit.CQRS.EventSourcing.Binary
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BinaryEventPayload
    {
        /// <summary>
        /// 
        /// </summary>
        public BinaryEventPayload() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public BinaryEventPayload(IEvent @event)
        {
            SourceID = @event.SourceID.ToString();
            ServiceID = @event.ServiceID;
            WorkerID = @event.WorkerID;

            Created = @event.Created;
            IsPubliched = false;

            BinaryFormatter formatter = new BinaryFormatter();
            using var ms = new MemoryStream();

            formatter.Serialize(ms, @event);
            this.RawData = ms.GetBuffer();
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
        public DateTime Created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPubliched { get; set; }

        /// <summary>
        /// Event Type.
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public void MarkEventAsPublished() => this.IsPubliched = true;
    }
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class BinaryVersionedEventPayload : BinaryEventPayload
    {
        /// <summary>
        /// 
        /// </summary>
        public BinaryVersionedEventPayload()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public BinaryVersionedEventPayload(IVersionedEvent @event)
            : base(@event)
        {
            Version = @event.Version;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }
    }
}
