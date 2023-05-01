using DotNetMQ.Client;
using DotNetMQ.Communication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.EventBus.Configuration;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.EventBus.DotNetMQ;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public class MemoryEventBus<TAlias> : IEventBus, IAsyncDisposable
    where TAlias : struct, Enum
{
    private MDSClient? _client;

    private readonly string _name;
    private readonly IEnumerable<QueueAlias<TAlias>> _queues;
    private readonly IEventNameResolver _resolver;
    private readonly IJsonSerializer _serializer;
    private readonly Func<TAlias, string, object, bool>? _filter;
    private readonly Func<TAlias, string, object, object>? _transform;
    private readonly Action<object, IOutgoingMessage>? _setup;
    private readonly ILogger<MemoryEventBus<TAlias>>? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="queues"></param>
    /// <param name="resolver"></param>
    /// <param name="serializer"></param>
    /// <param name="filter"></param>
    /// <param name="transform"></param>
    /// <param name="setup"></param>
    /// <param name="logger"></param>
    public MemoryEventBus(
        string name,
        IEnumerable<QueueAlias<TAlias>> queues,
        IEventNameResolver resolver,
        IJsonSerializer serializer,
        Func<TAlias, string, object, bool>? filter = null,
        Func<TAlias, string, object, object>? transform = null,
        Action<object, IOutgoingMessage>? setup = null,
        ILogger<MemoryEventBus<TAlias>>? logger = null
    )
    {
        _name = name;
        _queues = queues.Where(x => x.Active == true).ToArray();
        _filter = filter;
        _resolver = resolver;
        _serializer = serializer;
        _transform = transform;
        _setup = setup;
        _logger = logger;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_client is not null)
        {
            _client.Disconnect();
            _client.Dispose();
        }
        GC.SuppressFinalize(this);

#if NETSTANDARD2_0
        return new ValueTask(Task.CompletedTask);
#else
        return ValueTask.CompletedTask;
#endif
    }
    /// <inheritdoc />
    public ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        _logger?.LogDebug("MemoryEventBus start");

        _client = CreateClient(_name);
        _client.Connect();

#if NETSTANDARD2_0
        return new ValueTask(Task.CompletedTask);
#else
        return ValueTask.CompletedTask;
#endif
    }

    /// <inheritdoc />
    public Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default) => SendMessageAsync(@event, @event.Id, @event.Name, @event.CorrelationId, useEnvelop, ct);
    /// <inheritdoc />
    public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop = true, CancellationToken ct = default) => SendMessageAsync(graph, id, eventName, correlationId, useEnvelop, ct);
    /// <inheritdoc />
    public Task PublishPayloadAsync<TEventPayload>(TEventPayload @event, bool useEnvelop = true, CancellationToken ct = default) where TEventPayload : EventPayload
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create client 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    protected virtual MDSClient CreateClient(string name)
    {
        var client = new MDSClient(name)
        {
            CommunicationWay = CommunicationWays.Send
        };

        return client;
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
    private Task PublishMessageInQueueAsync(object graph, Guid id, string eventName, string? correlationId, QueueAlias<TAlias> queue, bool useEnvelop, CancellationToken ct)
    {
        if (_client is null)
            throw new InvalidOperationException("Bus is not started");

        var message = _client.CreateMessage();
        var transformed = _transform?.Invoke(queue.Alias, eventName, graph) ?? graph;

        var envelop = transformed;
        if (useEnvelop)
            envelop = new MessageEnvelop(transformed, transformed.GetType().FullName);

        var json = _serializer.Serialize(envelop)!;
        message.MessageData = Encoding.UTF8.GetBytes(json);

        //if (!string.IsNullOrEmpty(correlationId))
        //    message.CorrelationId = correlationId;

        message.MessageId = id.ToString();
        message.DestinationApplicationName = queue.Queue;
        //message.ContentType = "application/json";

        //if (graph is IDelayEvent delayEvent && delayEvent.Scheduled.HasValue)
        //    message.ScheduledEnqueueTime = delayEvent.Scheduled.Value;

        //message.ApplicationProperties[BusProperty.EventName] = eventName;
        //message.ApplicationProperties[BusProperty.MessageType] = transformed.GetType().AssemblyQualifiedName;
        _setup?.Invoke(graph, message);

        message.Send();
        _logger?.LogInformation("Publish to {Queue} event: {Event}", queue.Queue, json);

        return Task.CompletedTask;
    }
    #endregion
}
