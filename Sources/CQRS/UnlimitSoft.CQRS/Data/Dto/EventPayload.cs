using System;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Event;

namespace UnlimitSoft.CQRS.Data.Dto;


/// <summary>
/// Dto to represent an event in an storage
/// </summary>
public abstract class EventPayload
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// 
    /// </summary>
    protected EventPayload() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    protected EventPayload(IEvent @event)
    {
        Id = @event.Id;
        SourceId = @event.SourceId;
        Version = @event.Version;
        CorrelationId = @event.CorrelationId;
        EventName = @event.Name;
        Created = @event.Created;
        IsPubliched = false;
        if (@event is IDelayEvent delayEvent)
            Scheduled = delayEvent.Scheduled;
    }

    /// <summary>
    /// Event unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or set the identifier of the source originating the event.
    /// </summary>
    public Guid SourceId { get; set; }
    /// <summary>
    /// Gets the version or order of the event in the stream. Este valor lo asigna la entidad que lo genero y 
    /// es el que ella poseia en el instante en que fue generado el evento. 
    /// </summary>
    public long Version { get; set; }
    /// <summary>
    /// Event correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }
    /// <summary>
    /// Event unique name.
    /// </summary>
    public string EventName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// If the proviced 
    /// </summary>
    public DateTime? Scheduled { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsPubliched { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public void MarkEventAsPublished() => IsPubliched = true;

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EventPayload payload && Id == payload.Id;
}
/// <summary>
/// Dto to represent an event in an storage with a generic body depending of the serialization algorithm this value could change. 
/// See <see cref="JsonEventPayload"/> to visualize a body using json string
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public abstract class EventPayload<TPayload> : EventPayload
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// 
    /// </summary>
    protected EventPayload() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="payload"></param>
    protected EventPayload(IEvent @event, TPayload payload)
        : base(@event)
    {
        Payload = payload;
    }

    /// <summary>
    /// Complete event serialized in a payload. Serialization depends of the user approach could be json or binary
    /// </summary>
    public TPayload Payload { get; set; }

    /// <summary>
    /// Get event name inside the payload.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => EventName;
}
