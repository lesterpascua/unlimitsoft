using System;

namespace SoftUnlimit.CQRS.Event
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
        uint ServiceId { get; }
        /// <summary>
        /// Identifier of the worker were the event is create.
        /// </summary>
        string WorkerId { get; }
        /// <summary>
        /// Operation correlation identifier.
        /// </summary>
        public string CorrelationId { get; }
        /// <summary>
        /// Event creation date
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Event name general is the type fullname
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Command where event is originate (fullname).
        /// </summary>
        object Command { get; }
        /// <summary>
        /// Previous snapshot in json representation.
        /// </summary>
        object PrevState { get; }
        /// <summary>
        /// Currenct snapshot in json representation
        /// </summary>
        object CurrState { get; }
        /// <summary>
        /// Specify if an event belown to domain. This have optimization propouse.
        /// </summary>
        bool IsDomainEvent { get; }
        /// <summary>
        /// Event extra information
        /// </summary>
        public object Body { get; }
    }
    /// <summary>
    /// Represents an event message.
    /// </summary>
    public abstract class Event<TKey> : IEvent
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
        protected Event(Guid id, TKey sourceId, uint serviceId, string workerId, string correlationId, object command, object prevState, object currState, bool isDomain, object body)
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
        }

        /// <inheritdoc />
        public Guid Id { get; set; }
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public TKey SourceId { get; set; }
        /// <inheritdoc />
        public uint ServiceId { get; set; }
        /// <inheritdoc />
        public string WorkerId { get; set; }
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
        public object Body { get; set; }
        /// <inheritdoc />
        public string CorrelationId { get; set; }

        /// <inheritdoc />
        public override string ToString() => Name;

        #region Explicit Interface Implementation

        object IEvent.SourceId { get => SourceId; set => SourceId = (TKey)value; }

        #endregion
    }
}
