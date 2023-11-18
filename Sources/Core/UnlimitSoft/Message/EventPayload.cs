using System;
using UnlimitSoft.Json;

namespace UnlimitSoft.Message;


/// <summary>
/// Dto to represent an event in an storage
/// </summary>
public class EventPayload
{
    /// <summary>
    /// Deserialize constructor
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public EventPayload() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// Build a event payload from an event.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="serializer"></param>
    public EventPayload(IEvent @event, IJsonSerializer serializer)
    {
        Id = @event.Id;
        SourceId = @event.SourceId;
        Version = @event.Version;
        ServiceId = @event.ServiceId;
        WorkerId = @event.WorkerId;
        CorrelationId = @event.CorrelationId;
        Name = @event.Name;
        Created = @event.Created;
        IsDomainEvent = @event.IsDomainEvent;
        IsPubliched = false;
        if (@event is IDelayEvent delayEvent)
            Scheduled = delayEvent.Scheduled;

        Body = serializer.Serialize(@event.GetBody());
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
    /// Identifier of service where event is created
    /// </summary>
    public ushort ServiceId { get; set; }
    /// <summary>
    /// Identifier of the worker were the event is create.
    /// </summary>
    public string? WorkerId { get; set; }
    /// <summary>
    /// Event correlation identifier.
    /// </summary>
    public string? CorrelationId { get; set; }
    /// <summary>
    /// Event unique name.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime Created { get; set; }
    /// <summary>
    /// Specify if an event belown to domain. This have optimization propouse.
    /// </summary>
    public bool IsDomainEvent { get; set; }
    /// <summary>
    /// If the proviced 
    /// </summary>
    public DateTime? Scheduled { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool IsPubliched { get; set; }
    /// <summary>
    /// Body serialize as json object
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public void MarkEventAsPublished() => IsPubliched = true;

    /// <summary>
    /// Get event name inside the payload.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Name;
    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode();
    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EventPayload payload && Id == payload.Id;

    /// <summary>
    /// Create event from EventPayload
    /// </summary>
    /// <typeparam name="TEventPayload"></typeparam>
    /// <param name="eventType"></param>
    /// <param name="bodyType"></param>
    /// <param name="payload"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public static IEvent FromEventPayload<TEventPayload>(Type eventType, Type bodyType, TEventPayload payload, IJsonSerializer serializer) where TEventPayload : EventPayload
    {
        var e = (IEvent?)Activator.CreateInstance(eventType) ?? throw new InvalidOperationException("Can't create an event of this type");

        e.Id = payload.Id;
        e.SourceId = payload.SourceId;
        e.Version = payload.Version;
        e.ServiceId = payload.ServiceId;
        e.WorkerId = payload.WorkerId;
        e.CorrelationId = payload.CorrelationId;
        e.Name = payload.Name;
        e.Created = payload.Created;
        e.IsDomainEvent = payload.IsDomainEvent;

        var body = serializer.Deserialize(bodyType, payload.Body);
        e.SetBody(body);

        return e;
    }
}