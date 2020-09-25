using SoftUnlimit.CQRS.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBody"></typeparam>
    [Serializable]
    public abstract class EventPayload<TBody>
    {
        /// <summary>
        /// 
        /// </summary>
        protected EventPayload() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        protected EventPayload(IEvent @event)
        {
            var props = @event.Creator.GetProps<CommandProps>();
            CreatorId = props.Id;
            CreatorName = props.Name;

            SourceId = @event.SourceId.ToString();
            EntityName = (@event.CurrState ?? @event.PrevState)?.GetType().AssemblyQualifiedName;

            ServiceId = @event.ServiceId;
            WorkerId = @event.WorkerId;
            EventName = @event.Name;
            IsDomain = @event.IsDomainEvent;

            IsStartAction = @event is IStartActionEvent;
            IsFinalAction = @event is IFinalActionEvent;

            Created = @event.Created;
            IsPubliched = false;
        }

        /// <summary>
        /// Creator identifier (Command Id).
        /// </summary>
        public string CreatorId { get; set; }
        /// <summary>
        /// Creator name (Command FullName)
        /// </summary>
        public string CreatorName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Entity type full name.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Service unique identifier.
        /// </summary>
        public uint ServiceId { get; set; }
        /// <summary>
        /// Worker unique identifier per service.
        /// </summary>
        public string WorkerId { get; set; }

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
        public TBody Payload { get; set; }

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
}
