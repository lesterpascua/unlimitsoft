using UnlimitSoft.Event;
using System;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
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
        SourceId = @event.GetSourceId()?.ToString();
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
    /// 
    /// </summary>
    public string? SourceId { get; set; }
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
    public override bool Equals(object obj) => (obj is EventPayload payload) && Id == payload.Id;
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public abstract class EventPayload<TPayload> : EventPayload
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// 
    /// </summary>
    [System.Text.Json.Serialization.JsonConstructor]
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
    /// Event Type.
    /// </summary>
    public TPayload Payload { get; set; }

    /// <summary>
    /// Get event name inside the payload.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => EventName;
}
