using System;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
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
            Id = @event.Id;
            SourceId = @event.SourceId.ToString();
            CorrelationId = @event.CorrelationId;
            EventName = @event.Name;
            Created = @event.Created;
            IsPubliched = false;
        }

        /// <summary>
        /// Event unique identifier.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SourceId { get; set; }
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
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
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
        public void MarkEventAsPublished() => IsPubliched = true;
    }
}
