using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public abstract class EventPayload
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
            var currStateType = @event.CurrState?.GetType();
            var prevStateType = @event.PrevState?.GetType();
            if (currStateType != null && prevStateType != null && currStateType != prevStateType)
                throw new ArgumentException("Type of CurrState and PrevState not match", nameof(currStateType));

            Id = @event.Id;

            if (@event.Command is ICommand cmd)
            {
                var props = cmd?.GetProps<CommandProps>();
                CommandId = props?.Id ?? Guid.Empty;
            }
            CommandType = @event.Command?.GetType()?.AssemblyQualifiedName;

            SourceId = @event.SourceId.ToString();
            EntityType = (currStateType ?? prevStateType)?.AssemblyQualifiedName;

            ServiceId = @event.ServiceId;
            WorkerId = @event.WorkerId;

            CorrelationId = @event.CorrelationId;

            EventName = @event.Name;
            IsDomain = @event.IsDomainEvent;

            Created = @event.Created;
            IsPubliched = false;

            BodyType = @event.Body?.GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Event unique identifier.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Creator identifier (Command Id).
        /// </summary>
        public Guid CommandId { get; set; }
        /// <summary>
        /// Command string name responsible for the creation of this event (Command AssemblyQualifiedName)
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Entity string name inside currState and prevState (Entity AssemblyQualifiedName).
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Service unique identifier.
        /// </summary>
        public uint ServiceId { get; set; }
        /// <summary>
        /// Worker unique identifier per service.
        /// </summary>
        public string WorkerId { get; set; }
        /// <summary>
        /// Event correlation identifier.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Event unique name.
        /// </summary>
        public string EventName { get; set; }

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
        /// Type of the body.
        /// </summary>
        public string BodyType { get; set; }

        /// <summary>
        /// Resolve types for command, entity and body
        /// </summary>
        /// <param name="commandTypeName"></param>
        /// <param name="bodyTypeName"></param>
        /// <returns></returns>
        public static (Type, Type) ResolveType(string commandTypeName, string bodyTypeName)
        {
            Type commandType = null, bodyType = null;

            if (!string.IsNullOrEmpty(commandTypeName))
                commandType = Type.GetType(commandTypeName);
            if (!string.IsNullOrEmpty(bodyTypeName))
                bodyType = Type.GetType(bodyTypeName);

            return (commandType, bodyType);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    [Serializable]
    public abstract class EventPayload<TPayload> : EventPayload
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
            : base(@event)
        {
        }

        /// <summary>
        /// Event Type.
        /// </summary>
        public TPayload Payload { get; set; }

        /// <summary>
        /// Get event name inside the payload.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => EventName;
        /// <summary>
        /// 
        /// </summary>
        public void MarkEventAsPublished() => this.IsPubliched = true;
    }
}
