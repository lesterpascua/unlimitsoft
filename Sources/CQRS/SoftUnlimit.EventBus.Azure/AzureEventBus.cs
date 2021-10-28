﻿using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.EventBus.Azure.Configuration;
using SoftUnlimit.Json;
using SoftUnlimit.Web.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.EventBus.Azure
{
    /// <summary>
    /// Implement a bus to send message using azure resources.
    /// </summary>
    public class AzureEventBus<TAlias> : IEventBus, IAsyncDisposable
        where TAlias : Enum
    {
        private ServiceBusClient _client;
        private AsyncRetryPolicy _retryPolicy;

        private readonly IEnumerable<QueueAlias<TAlias>> _queues;
        private readonly string _endpoint;
        private readonly IEventNameResolver _eventNameResolver;
        private readonly Func<TAlias, string, object, bool> _filter;
        private readonly Func<TAlias, string, object, object> _transform;
        private readonly ILogger _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventNameResolver">Resolve real type of event using his name.</param>
        /// <param name="queues"></param>
        /// <param name="endpoint"></param>
        /// <param name="filter">Filter if this event able to sent to specifix queue, function (queueName, eventName) => bool</param>
        /// <param name="transform">Transform event into a diferent event (queueName, eventName, event) => event</param>
        /// <param name="logger"></param>
        public AzureEventBus(
            string endpoint,
            IEnumerable<QueueAlias<TAlias>> queues,
            IEventNameResolver eventNameResolver,
            Func<TAlias, string, object, bool> filter = null,
            Func<TAlias, string, object, object> transform = null,
            ILogger logger = null
        )
        {
            _queues = queues.Where(p => p.Active == true).ToArray();
            _filter = filter;
            _endpoint = endpoint;
            _eventNameResolver = eventNameResolver;
            _transform = transform;
            _logger = logger;
        }


        /// <inheritdoc/>
        public ValueTask DisposeAsync() => _client.DisposeAsync();
        /// <inheritdoc />
        public ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct = default)
        {
            _logger?.LogDebug("AzureEventBus start");

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => waitRetry,
                    (ex, time) => _logger?.LogWarning(ex, "Retry {Time} publish in EventBus, error: {Message}", time, ex.Message)
                );
            _client = new ServiceBusClient(_endpoint);

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc/>
        public Task PublishAsync(IEvent @event, CancellationToken ct = default) => SendMessageAsync(@event, @event.Id, @event.Name, @event.CorrelationId, MessageType.Event, ct);
        /// <inheritdoc/>
        public async Task PublishPayloadAsync<T>(EventPayload<T> eventPayload, MessageType type, CancellationToken ct = default)
        {
            switch (type)
            {
                case MessageType.Json:
                    await SendMessageAsync(eventPayload, eventPayload.Id, eventPayload.EventName, eventPayload.CorrelationId, type, ct);
                    break;
                case MessageType.Event:
                    if (eventPayload.Payload is not string payload)
                        throw new NotSupportedException("Only allow json payload");

                    var eventType = _eventNameResolver.Resolver(eventPayload.EventName);
                    if (eventType != null)
                    {
                        var @event = JsonUtility.Deserialize(eventType, payload);
                        await SendMessageAsync(@event, eventPayload.Id, eventPayload.EventName, eventPayload.CorrelationId, type, ct);
                    }
                    else
                        _logger?.LogWarning("Not found event {EventType}", eventPayload.EventName);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Raw object publush
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="id"></param>
        /// <param name="eventName"></param>
        /// <param name="correlationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, CancellationToken ct = default) => SendMessageAsync(graph, id, eventName, correlationId, MessageType.Event, ct);

        #region Private Method
        private async Task SendMessageAsync(object graph, Guid id, string eventName, string correlationId, MessageType type, CancellationToken ct)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var destQueues = _queues;
            if (_filter != null)
                destQueues = destQueues.Where(queue => _filter(queue.Alias, eventName, graph));

            var msgTasks = destQueues
                .Select(queue => PublishMessageInQueueAsync(graph, id, eventName, correlationId, type, queue, ct))
                .ToArray();
            //
            // Wait to send message over all queue 
            await Task.WhenAll(msgTasks);
        }
        private async Task PublishMessageInQueueAsync(object graph, Guid id, string eventName, string correlationId, MessageType type, QueueAlias<TAlias> queue, CancellationToken ct)
        {
            await using var sender = _client.CreateSender(queue.Queue);
            var transformed = _transform?.Invoke(queue.Alias, eventName, graph) ?? graph;

            var messageEnvelop = new MessageEnvelop
            {
                Type = type,
                Messaje = transformed,
                MessajeType = transformed.GetType().FullName
            };

            var json = JsonUtility.Serialize(messageEnvelop);
            var raw = Encoding.UTF8.GetBytes(json);

            var message = new ServiceBusMessage(raw);
            if (!string.IsNullOrEmpty(correlationId))
                message.CorrelationId = correlationId;

            message.MessageId = id.ToString();
            message.ContentType = "application/json";

            if (graph is IDelayEvent delayEvent)
            {
                var delay = delayEvent.GetDelay();
                if (delay.HasValue && delay != TimeSpan.Zero)
                    message.ScheduledEnqueueTime = DateTime.UtcNow.Add(delay.Value);
            }

            message.ApplicationProperties[BusProperty.EventName] = eventName;
            message.ApplicationProperties[BusProperty.MessageType] = transformed.GetType().AssemblyQualifiedName;

            await _retryPolicy.ExecuteAsync((cancelationToken) => sender.SendMessageAsync(message, cancelationToken), ct);
            _logger?.LogInformation("Publish to {Queue} event: {@Event}", queue.Queue, graph);
        }
        #endregion
    }
}