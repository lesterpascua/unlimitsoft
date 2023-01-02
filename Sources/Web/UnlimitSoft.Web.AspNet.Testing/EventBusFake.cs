using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data.Dto;

namespace UnlimitSoft.Web.AspNet.Testing;


/// <summary>
/// Fake event bus to simulate send event
/// </summary>
public class EventBusFake : IEventBus
{
    /// <inheritdoc />
    public ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct) => ValueTask.CompletedTask;
    /// <inheritdoc />
    public Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default)
    {
        Action?.Invoke(@event.Id, @event.Name, @event, @event.CorrelationId, ++EventArrive);
        return Task.CompletedTask;
    }
    /// <inheritdoc />
    public Task PublishPayloadAsync<T>(EventPayload<T> @event, MessageType type, bool useEnvelop = true, CancellationToken ct = default)
    {
        Action?.Invoke(@event.Id, @event.EventName, @event.Body, @event.CorrelationId, ++EventArrive);
        return Task.CompletedTask;
    }
    /// <inheritdoc />
    public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop = true, CancellationToken ct = default)
    {
        Action?.Invoke(id, eventName, graph, correlationId, ++EventArrive);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Index of the event, start in 0.
    /// </summary>
    public int EventArrive { get; set; }
    /// <summary>
    /// Callback function to notify some event arrive to the event bus. (eventId, eventName, eventPayload, correlationId, eventIndex)
    /// </summary>
    public Action<Guid, string, object, string, int> Action { get; set; }
}
