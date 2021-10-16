using SoftUnlimit.CQRS.Event;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Testing
{
    /// <summary>
    /// Fake event bus to simulate send event
    /// </summary>
    public class EventBusFake : IEventBus
    {
        /// <inheritdoc />
        public ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct) => ValueTask.CompletedTask;
        /// <inheritdoc />
        public Task PublishAsync(IEvent @event, CancellationToken ct = default)
        {
            Action?.Invoke(@event.Id, @event.Name, @event, @event.CorrelationId, ++EventArrive);
            return Task.CompletedTask;
        }
        /// <inheritdoc />
        public Task PublishPayloadAsync<T>(EventPayload<T> @event, MessageType type, CancellationToken ct = default)
        {
            Action?.Invoke(@event.Id, @event.EventName, @event.Payload, @event.CorrelationId, ++EventArrive);
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
}
