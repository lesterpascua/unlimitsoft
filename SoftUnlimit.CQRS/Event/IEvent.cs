using SoftUnlimit.CQRS.Command;
using SoftUnlimit.Map;
using SoftUnlimit.Web.Model;
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

        /// <summary>
        /// Transform event into other event with diferent entity type. Usefull to publish event without sensitive data or a public representation of the event.
        /// </summary>
        /// <param name="mapper"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        IEvent Transform(IMapper mapper, Type destination);
        /// <summary>
        /// Transform event into other event with diferent entity type. Usefull to publish event without sensitive data or a public representation of the event.
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="mapper"></param>
        /// <returns></returns>
        IEvent Transform<TDestination>(IMapper mapper) where TDestination : class;
    }
    /// <summary>
    /// Represents an event message.
    /// </summary>
    [Serializable]
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


        /// <inheritdoc />
        public virtual IEvent Transform(IMapper mapper, Type destination)
        {
            var currState = CurrState != null ? mapper.Map(CurrState, CurrState.GetType(), destination) : null;
            var prevState = PrevState != null ? mapper.Map(PrevState, PrevState.GetType(), destination) : null;

            var type = GetType();
            var @event = (IEvent)Activator.CreateInstance(type, Id, SourceId, ServiceId, WorkerId, CorrelationId, Command, prevState, currState, IsDomainEvent, Body);

            return @event;
        }
        /// <inheritdoc />
        public virtual IEvent Transform<TDestination>(IMapper mapper) where TDestination : class => Transform(mapper, typeof(TDestination));


        #region Explicit Interface Implementation

        object IEvent.SourceId { get => SourceId; set => SourceId = (TKey)value; }

        #endregion
    }

    /// <summary>
    /// Generic event use to deserialize any kind of event.
    /// </summary>
    [Serializable]
    public sealed class GenericEvent : Event<object>
    {
    }
}
