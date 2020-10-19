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
        ICommand Command { get; }
        /// <summary>
        /// Previous snapshot in json representation.
        /// </summary>
        IEntityInfo PrevState { get; }
        /// <summary>
        /// Currenct snapshot in json representation
        /// </summary>
        IEntityInfo CurrState { get; }
        /// <summary>
        /// Specify if an event belown to domain. This have optimization propouse.
        /// </summary>
        bool IsDomainEvent { get; }
        /// <summary>
        /// Event extra information
        /// </summary>
        public IEventBodyInfo Body { get; }

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
        IEvent Transform<TDestination>(IMapper mapper) where TDestination : class, IEntityInfo;
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
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="isDomain"></param>
        /// <param name="body"></param>
        protected Event(Guid id, TKey sourceId, uint serviceId, string workerId, ICommand command, IEntityInfo prevState, IEntityInfo currState, bool isDomain, IEventBodyInfo body)
        {
            Id = id;
            SourceId = sourceId;
            ServiceId = serviceId;
            WorkerId = workerId;

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
        public ICommand Command { get; set; }
        /// <inheritdoc />
        public IEntityInfo PrevState { get; set; }
        /// <inheritdoc />
        public IEntityInfo CurrState { get; set; }

        /// <inheritdoc />
        public bool IsDomainEvent { get; set; }
        /// <inheritdoc />
        public IEventBodyInfo Body { get; set; }

        /// <inheritdoc />
        public override string ToString() => Name;


        /// <inheritdoc />
        public virtual IEvent Transform(IMapper mapper, Type destination)
        {
            var currState = (IEntityInfo)mapper.Map(CurrState, CurrState.GetType(), destination);
            var prevState = (IEntityInfo)mapper.Map(PrevState, PrevState.GetType(), destination);

            var type = GetType();
            var @event = (IEvent)Activator.CreateInstance(type, Id, SourceId, ServiceId, WorkerId, Command, prevState, currState, IsDomainEvent, Body);

            return @event;
        }
        /// <inheritdoc />
        public virtual IEvent Transform<TDestination>(IMapper mapper) where TDestination : class, IEntityInfo => Transform(mapper, typeof(TDestination));


        #region Explicit Interface Implementation

        object IEvent.SourceId { get => SourceId; set => SourceId = (TKey)value; }

        #endregion
    }
}
