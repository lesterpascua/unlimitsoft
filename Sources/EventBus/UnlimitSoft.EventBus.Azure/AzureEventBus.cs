using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using UnlimitSoft.EventBus.Configuration;
using UnlimitSoft.Json;

namespace UnlimitSoft.EventBus.Azure;


/// <summary>
/// Implement a bus to send message using azure resources.
/// </summary>
public class AzureEventBus<TAlias> : IEventBus, IAsyncDisposable
    where TAlias : struct, Enum
{
    private ServiceBusClient? _client;
    private AsyncRetryPolicy? _retryPolicy;

    private readonly IEnumerable<QueueAlias<TAlias>> _queues;
    private readonly string _endpoint;
    private readonly IEventNameResolver _eventNameResolver;
    private readonly IJsonSerializer _serializer;
    private readonly Func<TAlias, string, object, bool>? _filter;
    private readonly Func<TAlias, string, object, object>? _transform;
    private readonly Action<object, ServiceBusMessage>? _setup;
    private readonly ILogger<AzureEventBus<TAlias>>? _logger;


    /// <summary>
    /// initialize new instance of the azure event bus.
    /// </summary>
    /// <param name="endpoint">Connection string to azure event bus. Endpoint=sb://my.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=supersecretsharedkey</param>
    /// <param name="queues">Queues where the current bus can publish events. This is a collection of all availables queue later the system can select the queue to publish every specific event using filter argument</param>
    /// <param name="eventNameResolver">Resolve real type of event using his name.</param>
    /// <param name="serializer">Json serializer used to serializer the event</param>
    /// <param name="filter">Filter if this event able to sent to specifix queue, function (alias, eventName, event) => bool</param>
    /// <param name="transform">Transform event into a diferent event (alias, eventName, event) => event</param>
    /// <param name="setup">Allow custom setup message before send to the bus</param>
    /// <param name="logger">Logger used to register process data</param>
    public AzureEventBus(
        string endpoint,
        IEnumerable<QueueAlias<TAlias>> queues,
        IEventNameResolver eventNameResolver,
        IJsonSerializer serializer,
        Func<TAlias, string, object, bool>? filter = null,
        Func<TAlias, string, object, object>? transform = null,
        Action<object, ServiceBusMessage>? setup = null,
        ILogger<AzureEventBus<TAlias>>? logger = null
    )
    {
        _queues = queues.Where(x => x.Active == true).ToArray();
        _filter = filter;
        _endpoint = endpoint;
        _eventNameResolver = eventNameResolver;
        _serializer = serializer;
        _transform = transform;
        _setup = setup;
        _logger = logger;
    }


    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
            await _client.DisposeAsync();
        GC.SuppressFinalize(this);
    }
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
        _client = CreateClient();

#if NETSTANDARD2_0
        return ValueTaskExtensions.CompletedTask;
#else
        return ValueTask.CompletedTask;
#endif
    }

    /// <inheritdoc/>
    public Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default) => SendMessageAsync(@event, @event.Id, @event.Name, @event.CorrelationId, useEnvelop, ct);
    /// <inheritdoc/>
    public async Task PublishPayloadAsync<T>(EventPayload<T> eventPayload, MessageType type, bool useEnvelop = true, CancellationToken ct = default)
    {
        switch (type)
        {
            case MessageType.Json:
                await SendMessageAsync(eventPayload, eventPayload.Id, eventPayload.EventName, eventPayload.CorrelationId, useEnvelop, ct);
                break;
            case MessageType.Event:
                if (eventPayload.Payload is not string payload)
                    throw new NotSupportedException("Only allow json payload");

                var eventType = _eventNameResolver.Resolver(eventPayload.EventName);
                if (eventType is null)
                {
                    _logger?.LogWarning("Not found event {EventType}", eventPayload.EventName);
                    break;
                }
                var @event = _serializer.Deserialize(eventType, payload);
                if (@event is null)
                {
                    _logger?.LogWarning("Skip event of {Type} because is null", eventType);
                    break;
                }
                await SendMessageAsync(@event, eventPayload.Id, eventPayload.EventName, eventPayload.CorrelationId, useEnvelop, ct);
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
    /// <param name="useEnvelop"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop, CancellationToken ct = default) => SendMessageAsync(graph, id, eventName, correlationId, useEnvelop, ct);

    /// <summary>
    /// Create instance of the service bus client
    /// </summary>
    /// <returns></returns>
    protected virtual ServiceBusClient CreateClient()
    {
        return new ServiceBusClient(_endpoint);
    }

    #region Private Method
    private async Task SendMessageAsync(object graph, Guid id, string eventName, string? correlationId, bool useEnvelop, CancellationToken ct)
    {
        if (graph is null)
            throw new ArgumentNullException(nameof(graph));

        var destQueues = _queues;
        if (_filter is not null)
            destQueues = destQueues.Where(queue => _filter(queue.Alias, eventName, graph));

        var msgTasks = destQueues
            .Select(queue => PublishMessageInQueueAsync(graph, id, eventName, correlationId, queue, useEnvelop, ct))
            .ToArray();
        //
        // Wait to send message over all queue 
        await Task.WhenAll(msgTasks);
    }
    private async Task PublishMessageInQueueAsync(object graph, Guid id, string eventName, string? correlationId, QueueAlias<TAlias> queue, bool useEnvelop, CancellationToken ct)
    {
        if (_client is null || _retryPolicy is null)
            throw new InvalidOperationException("Bus is not started");

        await using var sender = _client.CreateSender(queue.Queue);
        var transformed = _transform?.Invoke(queue.Alias, eventName, graph) ?? graph;

        var envelop = transformed;
        if (useEnvelop)
            envelop = new MessageEnvelop(MessageType.Json, transformed, transformed.GetType().FullName);

        var json = _serializer.Serialize(envelop)!;

        var message = new ServiceBusMessage(json);
        if (!string.IsNullOrEmpty(correlationId))
            message.CorrelationId = correlationId;

        message.MessageId = id.ToString();
        message.ContentType = "application/json";

        if (graph is IDelayEvent delayEvent && delayEvent.Scheduled.HasValue)
            message.ScheduledEnqueueTime = delayEvent.Scheduled.Value;

        message.ApplicationProperties[BusProperty.EventName] = eventName;
        message.ApplicationProperties[BusProperty.MessageType] = transformed.GetType().AssemblyQualifiedName;
        _setup?.Invoke(graph, message);

        await _retryPolicy.ExecuteAsync((cancelationToken) => sender.SendMessageAsync(message, cancelationToken), ct);
        _logger?.LogInformation("Publish to {Queue} event: {Event}", queue.Queue, json);
    }
    #endregion
}
