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
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.EventBus.RabbitMQ;


/// <summary>
/// 
/// </summary>
public class RabbitMQEventBus<TAlias, TEventPayload> : IEventBus, IDisposable 
    where TAlias : struct, Enum
    where TEventPayload: EventPayload
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
    private readonly ILogger<RabbitMQEventBus<TAlias, TEventPayload>>? _logger;

    /// <summary>
    /// RabbitMQ model
    /// </summary>
    protected IModel? _channel;
    /// <summary>
    /// RabbitMQ connection
    /// </summary>
    protected IConnection? _connection;

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
        ILogger<RabbitMQEventBus<TAlias, TEventPayload>>? logger = null
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
        ILogger<RabbitMQEventBus<TAlias, TEventPayload>>? logger = null
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
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        return default;
    }

    /// <inheritdoc />
    public Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default)
    {
        return SendAsync(@event, @event.Id, @event.Name, @event.CorrelationId, useEnvelop);
    }
    /// <inheritdoc />
    public virtual Task PublishPayloadAsync(EventPayload eventPayload, bool useEnvelop = true, CancellationToken ct = default)
    {
        var eventType = _resolver.Resolver(eventPayload.Name);
        if (eventType is null)
            return Task.CompletedTask;

        var @event = LoadFromPaylod(eventType, (TEventPayload)eventPayload);
        if (@event is null)
            return Task.CompletedTask;
        return SendAsync(@event, eventPayload.Id, eventPayload.Name, eventPayload.CorrelationId, useEnvelop);
    }
    /// <inheritdoc />
    public Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop = true, CancellationToken ct = default) => SendAsync(graph, id, eventName, correlationId, useEnvelop);

    /// <summary>
    /// Load event from Payload
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    protected virtual IEvent? LoadFromPaylod(Type eventType, TEventPayload payload)
    {
        var bodyType = _resolver.GetBodyType(eventType);
        return EventPayload.FromEventPayload(eventType, bodyType, payload, _serializer);
    }
    /// <summary>
    /// Send message async
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="id"></param>
    /// <param name="eventName"></param>
    /// <param name="correlationId"></param>
    /// <param name="useEnvelop"></param>
    /// <returns></returns>
    protected Task SendAsync(object graph, Guid id, string eventName, string? correlationId, bool useEnvelop)
    {
        //using var rabbitMQConnection = _retryPolicy.Execute(() => _factory.CreateConnection());
        //rabbitMQConnection.CreateModel()

        _channel!.ConfirmSelect();
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = id.ToString();

        properties.Headers ??= new Dictionary<string, object>();
        properties.Headers[SysContants.HeaderCorrelation] = correlationId;
        properties.Headers[Constants.HeaderEventName] = eventName;
        properties.Headers[Constants.HeaderHasEnvelop] = useEnvelop;

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
            foreach (var rk in queue.RoutingKey)
            {
                if (useEnvelop)
                    content = new MessageEnvelop(content, _resolver.Resolver(content.GetType()) ?? rk);

                var message = Encoding.UTF8.GetBytes(_serializer.Serialize(content)!);

                _channel.BasicPublish(queue.Exchange, rk, true, properties, message);
                _channel.WaitForConfirms();

                _logger?.LogInformation("Publish Event to {Exchange}, {RoutingKey}, {@Event}", queue.Exchange, queue.RoutingKey, graph);
            }
        }
        return Task.CompletedTask;
    }
}