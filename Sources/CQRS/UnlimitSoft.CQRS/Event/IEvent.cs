using System;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.Event;


/// <summary>
/// Interface of the events in the systems
/// </summary>
public interface IEvent : IRequest<IResponse>
{
    /// <summary>
    /// Unique identifier of the event.
    /// </summary>
    Guid Id { get; set; }
    /// <summary>
    /// Gets or set the identifier of the source originating the event.
    /// </summary>
    Guid SourceId { get; set; }
    /// <summary>
    /// Gets the version or order of the event in the stream. Este valor lo asigna la entidad que lo genero y 
    /// es el que ella poseia en el instante en que fue generado el evento. 
    /// </summary>
    long Version { get; set; }
    /// <summary>
    /// Identifier of service where event is created
    /// </summary>
    ushort ServiceId { get; set; }
    /// <summary>
    /// Identifier of the worker were the event is create.
    /// </summary>
    string? WorkerId { get; set; }
    /// <summary>
    /// Operation correlation identifier.
    /// </summary>
    string? CorrelationId { get; set; }
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
    object? Command { get; set; }
    /// <summary>
    /// Previous snapshot in json representation.
    /// </summary>
    object? PrevState { get; set; }
    /// <summary>
    /// Currenct snapshot in json representation
    /// </summary>
    object? CurrState { get; set; }
    /// <summary>
    /// Specify if an event belown to domain. This have optimization propouse.
    /// </summary>
    bool IsDomainEvent { get; set; }

    /// <summary>
    /// Get event body.
    /// </summary>
    /// <returns></returns>
    object? GetBody();
}
/// <summary>
/// Represents an event message.
/// </summary>
public abstract class Event<TBody> : IEvent
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    /// <summary>
    /// Deserialization constructor
    /// </summary>
    protected Event()
    {
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sourceId"></param>
    /// <param name="version"></param>
    /// <param name="serviceId"></param>
    /// <param name="workerId"></param>
    /// <param name="correlationId"></param>
    /// <param name="command"></param>
    /// <param name="prevState"></param>
    /// <param name="currState"></param>
    /// <param name="isDomain"></param>
    /// <param name="body"></param>
    protected Event(Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, object? command, object? prevState, object? currState, bool isDomain, TBody? body)
    {
        Id = id;
        SourceId = sourceId;
        Version = version;
        ServiceId = serviceId;
        WorkerId = workerId;
        CorrelationId = correlationId;
        Name = GetType().FullName!;

        Command = command;
        PrevState = prevState;
        CurrState = currState;

        IsDomainEvent = isDomain;
        Body = body;

        Created = SysClock.GetUtcNow();
    }

    /// <inheritdoc />
    public Guid Id { get; set; }
    /// <inheritdoc />
    public Guid SourceId { get; set; }
    /// <inheritdoc />
    public long Version { get; set; }
    /// <inheritdoc />
    public ushort ServiceId { get; set; }
    /// <inheritdoc />
    public string? WorkerId { get; set; }
    /// <inheritdoc />
    public TimeSpan? Delay { get; set; }
    /// <inheritdoc />
    public DateTime Created { get; set; }
    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public object? Command { get; set; }
    /// <inheritdoc />
    public object? PrevState { get; set; }
    /// <inheritdoc />
    public object? CurrState { get; set; }

    /// <inheritdoc />
    public bool IsDomainEvent { get; set; }
    /// <inheritdoc />
    public TBody? Body { get; set; }
    /// <inheritdoc />
    public string? CorrelationId { get; set; }

    /// <inheritdoc />
    public object? GetBody() => Body;

    /// <inheritdoc />
    public override string ToString() => Name;
}
