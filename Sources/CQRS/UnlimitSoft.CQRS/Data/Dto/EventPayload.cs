using UnlimitSoft.Event;

/* Unmerged change from project 'UnlimitSoft.CQRS (net7.0)'
Before:
using System;
After:
using System;
using UnlimitSoft;
using UnlimitSoft.CQRS;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Data.Dto;
*/
using System;
using UnlimitSoft.CQRS.Event;

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
    /// 
    /// </summary>
    public Guid SourceId { get; set; }
    /// <summary>
    /// 
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
/// 
/// </summary>
/// <typeparam name="TBody"></typeparam>
public abstract class EventPayload<TBody> : EventPayload
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
    /// <param name="body"></param>
    protected EventPayload(IEvent @event, TBody body)
        : base(@event)
    {
        Body = body;
    }

    /// <summary>
    /// Event Type.
    /// </summary>
    public TBody Body { get; set; }

    /// <summary>
    /// Get event name inside the payload.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => EventName;
}
