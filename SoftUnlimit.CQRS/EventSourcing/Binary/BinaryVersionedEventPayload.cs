using Newtonsoft.Json;
using SoftUnlimit.CQRS.Command;
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
            var props = @event.Creator.GetProps<CommandProps>();
            CreatorID = props.Id;
            CreatorName = props.Name;

            SourceID = @event.SourceID.ToString();
            ServiceID = @event.ServiceID;
            WorkerID = @event.WorkerID;
            EventName = @event.Name;
            IsDomain = @event.IsDomainEvent;

            IsStartAction = @event is IStartActionEvent;
            IsFinalAction = @event is IFinalActionEvent;

            Created = @event.Created;
            IsPubliched = false;

            BinaryFormatter formatter = new BinaryFormatter();
            using var ms = new MemoryStream();

            formatter.Serialize(ms, @event);
            this.RawData = ms.GetBuffer();
        }

        /// <summary>
        /// Creator identifier (Command Id).
        /// </summary>
        public string CreatorID { get; set; }
        /// <summary>
        /// Creator name (Command FullName)
        /// </summary>
        public string CreatorName { get; set; }

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
        /// Event unique name.
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// This event is the first event in action it's generate directly by a command.
        /// </summary>
        public bool IsStartAction { get; set; }
        /// <summary>
        /// This event is the final event in action can generate response.
        /// </summary>
        public bool IsFinalAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Created { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsPubliched { get; set; }
        /// <summary>
        /// If mark as domain event only has sence inside this microservice. Never publich to other events.
        /// </summary>
        public bool IsDomain { get; set; }

        /// <summary>
        /// Event Type.
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// Convert objeto to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => EventName;
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
