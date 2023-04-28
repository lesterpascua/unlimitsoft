using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using UnlimitSoft.Json;

namespace UnlimitSoft.EventBus.RabbitMQ;


/// <summary>
/// 
/// </summary>
public class RabbitMQEventBus<TAlias> : IEventBus, IDisposable where TAlias : struct, Enum
{
    private readonly IConnectionFactory _factory;
    private readonly IEnumerable<RabbitMQQueueAlias<TAlias>> _queue;
    private readonly IEventNameResolver _resolver;
    private readonly IJsonSerializer _serializer;
    private readonly Func<TAlias, string, object, bool>? _filter;
    private readonly Func<TAlias, string, object, object>? _transform;
    private readonly Action<object?, BasicAckEventArgs, IEvent?>? _acks;
    private readonly Action<object?, BasicNackEventArgs, IEvent?>? _nacks;
    private readonly Action<object, IBasicProperties>? _setup;
    private readonly ILogger<RabbitMQEventBus<TAlias>>? _logger;

    private IModel? _channel;
    private IConnection? _connection;

    //private readonly RetryPolicy _retryPolicy;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="queue"></param>
    /// <param name="resolver"></param>
    /// <param name="serializer"></param>
    /// <param name="filter"></param>
    /// <param name="transform"></param>
    /// <param name="setup"></param>
    /// <param name="logger"></param>
    public RabbitMQEventBus(
        string endpoint,
        IEnumerable<RabbitMQQueueAlias<TAlias>> queue,
        IEventNameResolver resolver,
        IJsonSerializer serializer,
        Func<TAlias, string, object, bool>? filter = null,
        Func<TAlias, string, object, object>? transform = null,
        Action<object, IBasicProperties>? setup = null,
        ILogger<RabbitMQEventBus<TAlias>>? logger = null
    ) : this(new ConnectionFactory { Uri = new Uri(endpoint) }, queue, resolver, serializer, filter, transform, null, null, setup, logger)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="queue"></param>
    /// <param name="resolver"></param>
    /// <param name="serializer"></param>
    /// <param name="filter"></param>
    /// <param name="transform"></param>
    /// <param name="acks"></param>
    /// <param name="nacks"></param>
    /// <param name="setup"></param>
    /// <param name="logger"></param>
    public RabbitMQEventBus(IConnectionFactory factory, IEnumerable<RabbitMQQueueAlias<TAlias>> queue, IEventNameResolver resolver, IJsonSerializer serializer,
        Func<TAlias, string, object, bool>? filter = null, Func<TAlias, string, object, object>? transform = null,
        Action<object?, BasicAckEventArgs, IEvent?>? acks = null, Action<object?, BasicNackEventArgs, IEvent?>? nacks = null,
        Action<object, IBasicProperties>? setup = null,
        ILogger<RabbitMQEventBus<TAlias>>? logger = null
    )
    {
        _factory = factory;
        _queue = queue.Where(x => x.Active == true).ToArray();
        _resolver = resolver;
        _serializer = serializer;
        _filter = filter;
        _transform = transform;
        _acks = acks;
        _nacks = nacks;
        _setup = setup;
        _logger = logger;

        //_retryPolicy = Policy.Handle<BrokerUnreachableException>()
        //.Or<SocketException>()
        //        .WaitAndRetry(options.Value.RetryAttempts, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (ex, time) => logger.LogWarning(ex, "Error connecting to RabbitMQ"));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
    /// <inheritdoc />
    public ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        return default;
    }

    /// <inheritdoc />
    public Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default)
    {
        return SendAsync(@event, @event.Id, @event.Name, @event.CorrelationId, useEnvelop);
    }
    /// <inheritdoc />
    public Task PublishPayloadAsync<T>(EventPayload<T> eventPayload, bool useEnvelop = true, CancellationToken ct = default)
    {
        throw new NotImplementedException();
        //var eventType = _resolver.Resolver(eventPayload.EventName);
        //if (eventType is null)
        //{
        //    _logger?.LogWarning("Not found event {EventType}", eventPayload.EventName);
        //    return Task.CompletedTask;
        //}
        //var @event = LoadFromPaylod(eventType, eventPayload.Payload);
        //return Task.CompletedTask;
    }
    /// <inheritdoc />
    public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop = true, CancellationToken ct = default) => SendAsync(graph, id, eventName, correlationId, useEnvelop);

    /// <summary>
    /// Load event from Payload
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventType"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected virtual object? LoadFromPaylod<T>(Type eventType, T payload)
    {
        if (payload is not string json)
            throw new NotSupportedException("Only allow json payload");
        return _serializer.Deserialize(eventType, json);
    }

    #region Private Methods
    private Task SendAsync(object graph, Guid id, string eventName, string? correlationId, bool useEnvelop)
    {
        //using var rabbitMQConnection = _retryPolicy.Execute(() => _factory.CreateConnection());
        //rabbitMQConnection.CreateModel()

        _channel!.ConfirmSelect();
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = id.ToString();

        properties.Headers ??= new Dictionary<string, object>();
        properties.Headers.Add(SysContants.HeaderCorrelation, correlationId);
        properties.Headers.Add(Constanst.HeaderHasEnvelop, useEnvelop);

        _setup?.Invoke(graph, properties);

        // 
        // code when message is confirmed
        _channel.BasicAcks += (sender, ea) =>
        {
            _logger?.LogInformation("Acks: {Event}", id);
            _acks?.Invoke(sender, ea, graph as IEvent);
        };
        _channel.BasicNacks += (sender, ea) =>
        {
            _logger?.LogInformation("Nacks: {Event}", id);
            _nacks?.Invoke(sender, ea, graph as IEvent);
        };

        var destQueues = _queue;
        if (_filter is not null)
            destQueues = destQueues.Where(queue => _filter(queue.Alias, eventName, graph));

        foreach (var queue in destQueues)
        {
            var content = _transform?.Invoke(queue.Alias, eventName, graph) ?? graph;
            if (useEnvelop)
                content = new MessageEnvelop(content, queue.RoutingKey);    // use resolve event in reverse to 

            var message = Encoding.UTF8.GetBytes(_serializer.Serialize(content)!);

            _channel.BasicPublish(queue.Exchange, queue.RoutingKey, true, properties, message);
            _channel.WaitForConfirms();

            _logger?.LogInformation("Publish to {Exchange}, RoutingKey {Key}, {@Event}", queue.Exchange, queue.RoutingKey, graph);
        }
        return Task.CompletedTask;
    }
    #endregion
}