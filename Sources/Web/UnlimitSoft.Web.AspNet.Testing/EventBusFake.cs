using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.AspNet.Testing;


/// <summary>
/// Fake event bus to simulate send event
/// </summary>
public class EventBusFake : IEventBus
{
    private int _eventArrive;

    /// <inheritdoc />
    public ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct) => ValueTask.CompletedTask;
    /// <inheritdoc />
    public Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default)
    {
        Action?.Invoke(@event.Id, @event.Name, @event, @event.CorrelationId, Interlocked.Add(ref _eventArrive, 1));
        return Task.CompletedTask;
    }
    /// <inheritdoc />
    public Task PublishPayloadAsync(EventPayload @event, bool useEnvelop = true, CancellationToken ct = default)
    {
        Action?.Invoke(@event.Id, @event.Name, @event, @event.CorrelationId, Interlocked.Add(ref _eventArrive, 1));
        return Task.CompletedTask;
    }
    /// <inheritdoc />
    public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop = true, CancellationToken ct = default)
    {
        Action?.Invoke(id, eventName, graph, correlationId, Interlocked.Add(ref _eventArrive, 1));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Index of the event, start in 0.
    /// </summary>
    public int EventArrive => _eventArrive;
    /// <summary>
    /// Callback function to notify some event arrive to the event bus. (eventId, eventName, eventPayload, correlationId, eventIndex)
    /// </summary>
    public Action<Guid, string, object, string?, int>? Action { get; set; }
}
