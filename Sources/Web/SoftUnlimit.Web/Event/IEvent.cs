using System;

namespace SoftUnlimit.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Command creator identifier.
        /// </summary>
        Guid Id { get; set; }
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        object SourceId { get; set; }
        /// <summary>
        /// Identifier of service where event is created
        /// </summary>
        ushort ServiceId { get; set; }
        /// <summary>
        /// Identifier of the worker were the event is create.
        /// </summary>
        string WorkerId { get; set; }
        /// <summary>
        /// Operation correlation identifier.
        /// </summary>
        string CorrelationId { get; set; }
        /// <summary>
        /// Event creation date
        /// </summary>
        DateTime Created { get; set; }
        /// <summary>
        /// Event name general is the type fullname
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Command where event is originate (fullname).
        /// </summary>
        object Command { get; set; }
        /// <summary>
        /// Previous snapshot in json representation.
        /// </summary>
        object PrevState { get; set; }
        /// <summary>
        /// Currenct snapshot in json representation
        /// </summary>
        object CurrState { get; set; }
        /// <summary>
        /// Specify if an event belown to domain. This have optimization propouse.
        /// </summary>
        bool IsDomainEvent { get; set; }

        /// <summary>
        /// Get event body.
        /// </summary>
        /// <returns></returns>
        object GetBody();
    }
    /// <summary>
    /// Represents an event message.
    /// </summary>
    public abstract class Event<TKey, TBody> : IEvent
    {
        /// <summary>
        /// 
        /// </summary>
        protected Event()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sourceId"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="correlationId"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="isDomain"></param>
        /// <param name="body"></param>
        protected Event(Guid id, TKey sourceId, ushort serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomain, TBody body)
        {
            Id = id;
            SourceId = sourceId;
            ServiceId = serviceId;
            WorkerId = workerId;
            CorrelationId = correlationId;
            Name = GetType().FullName;

            Command = command;
            PrevState = prevState;
            CurrState = currState;

            IsDomainEvent = isDomain;
            Body = body;

            Created = DateTime.UtcNow;
        }

        /// <inheritdoc />
        public Guid Id { get; set; }
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public TKey SourceId { get; set; }
        /// <inheritdoc />
        public ushort ServiceId { get; set; }
        /// <inheritdoc />
        public string WorkerId { get; set; }
        /// <inheritdoc />
        public TimeSpan? Delay { get; set; }
        /// <inheritdoc />
        public DateTime Created { get; set; }
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public object Command { get; set; }
        /// <inheritdoc />
        public object PrevState { get; set; }
        /// <inheritdoc />
        public object CurrState { get; set; }

        /// <inheritdoc />
        public bool IsDomainEvent { get; set; }
        /// <inheritdoc />
        public TBody Body { get; set; }
        /// <inheritdoc />
        public string CorrelationId { get; set; }

        /// <inheritdoc />
        public object GetBody() => Body;
        /// <inheritdoc />
        public override string ToString() => Name;


        #region Explicit Interface Implementation

        object IEvent.SourceId { get => SourceId; set => SourceId = (TKey)value; }

        #endregion
    }
}
