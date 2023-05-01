using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Data;


/// <summary>
/// Represents an identifiable entity that is event sourced.
/// </summary>
public interface IEventSourced : IAggregateRoot
{
    /// <summary>
    /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
    /// </summary>
    long Version { get; }

    /// <summary>
    /// Gets the collection of all events used to build the entity. This is all operation over this entity until the curr moment.
    /// </summary>
    /// <param name="paging">Allow paging event, null to retrieve all</param>
    /// <returns></returns>
    IReadOnlyCollection<IEvent>? GetHistoricalEvents(Paging? paging = null);
}
/// <summary>
/// Base class for event sourced entities that implements <see cref="IEventSourced"/>. 
/// </summary>
/// <remarks>
/// <see cref="IEventSourced"/> entities do not require the use of <see cref="EventSourced"/>, but this class contains some common 
/// useful functionality related to versions and rehydration from past events.
/// </remarks>
public abstract class EventSourced : AggregateRoot, IEventSourced
{
    private long _version;
    private readonly IReadOnlyCollection<IEvent>? _historicalEvents;


    /// <summary>
    /// 
    /// </summary>
    protected EventSourced()
    {
        _version = -1;
        _historicalEvents = null;
    }
    /// <summary>
    /// Create a Event Sourced initial amount of event
    /// </summary>
    /// <param name="historicalEvents">List of event asociate to the entity at this moment.</param>
    protected EventSourced(IReadOnlyCollection<IEvent>? historicalEvents)
        : this()
    {
        if (historicalEvents?.Any() == true)
        {
            _historicalEvents = historicalEvents;
            _version = historicalEvents.Max(p => p.Version);
        }
    }

    #region Public Properties

    /// <summary>
    /// Gets the entity's version. As the entity is being updated and events being generated, the version is incremented.
    /// </summary>
    public long Version { get => _version; set => _version = value; }

    /// <inheritdoc />
    public IReadOnlyCollection<IEvent>? GetHistoricalEvents(Paging? paging)
    {
        if (paging is null || _historicalEvents is null)
            return _historicalEvents;
        return _historicalEvents.Skip(paging.Page * paging.PageSize).Take(paging.PageSize).ToArray();
    }

    /// <summary>
    /// Attach the event to the entity
    /// </summary>
    /// <param name="event"></param>
    protected void AddEvent(IEvent @event) => _events.Add(@event);
    /// <summary>
    /// Create event of the type specified and attach the event to the entity
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="serviceId"></param>
    /// <param name="workerId">Worker identifier. Can't be null.</param>
    /// <param name="correlationId">Correlation identifier to trace the events.</param>
    /// <param name="body"></param>
    protected TEvent AddEvent<TEvent, TBody>(Guid eventId, ushort serviceId, string? workerId, string? correlationId, TBody body) where TEvent : Event<TBody>
    {
        var args = new EventFactoryArgs<TBody>
        {
            EventType = typeof(TEvent),
            EventId = eventId,
            SourceId = Id,
            Version = Interlocked.Increment(ref _version),
            ServiceId = serviceId,
            WorkerId = workerId,
            CorrelationId = correlationId,
            IsDomain = false,
            Body = body
        };
        var @event = (TEvent)EventFactory(in args);

        AddEvent(@event);
        return @event;
    }

    /// <summary>
    /// Create versioned event factory
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    protected virtual IEvent EventFactory<TBody>(in EventFactoryArgs<TBody> args)
    {
        var @event = Activator.CreateInstance(
            args.EventType,
            args.EventId,
            args.SourceId,
            args.Version,
            args.ServiceId,
            args.WorkerId,
            args.CorrelationId,
            args.IsDomain,
            args.Body
        ) ?? throw new InvalidOperationException("Not able to build event of type");

        return (IEvent)@event;
    }

    #endregion

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public readonly struct EventFactoryArgs<TBody>
    {
        /// <summary>
        /// 
        /// </summary>
        public Type EventType { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public Guid EventId { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public Guid SourceId { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public long Version { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public ushort ServiceId { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public string? WorkerId { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public string? CorrelationId { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDomain { get; init; }
        /// <summary>
        /// 
        /// </summary>
        public TBody Body { get; init; }
    }
    #endregion
}
