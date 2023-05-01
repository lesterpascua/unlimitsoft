using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Json;

namespace UnlimitSoft.EventBus.RabbitMQ;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public class RabbitMQEventListener<TAlias> : IEventListener, IDisposable where TAlias : struct, Enum
{
    private readonly IConnectionFactory _factory;
    private readonly IJsonSerializer _serializer;
    private readonly IEnumerable<RabbitMQQueueAlias<TAlias>> _queues;
    private readonly ProcessorCallback<TAlias, RabbitMQBody> _processor;
    private readonly bool _recreateQueue;
    private readonly ILogger<RabbitMQEventListener<TAlias>>? _logger;

    private IModel? _channel;
    private IConnection? _connection;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="serializer"></param>
    /// <param name="queues"></param>
    /// <param name="processor"></param>
    /// <param name="recreateQueue"></param>
    /// <param name="logger"></param>
    public RabbitMQEventListener(string endpoint, IJsonSerializer serializer, IEnumerable<RabbitMQQueueAlias<TAlias>> queues, ProcessorCallback<TAlias, RabbitMQBody> processor, bool recreateQueue = false, ILogger<RabbitMQEventListener<TAlias>>? logger = null)
        : this(new ConnectionFactory { Uri = new Uri(endpoint) }, serializer, queues, processor, recreateQueue, logger)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="serializer"></param>
    /// <param name="queues"></param>
    /// <param name="processor"></param>
    /// <param name="recreateQueue"></param>
    /// <param name="logger"></param>
    public RabbitMQEventListener(IConnectionFactory factory, IJsonSerializer serializer, IEnumerable<RabbitMQQueueAlias<TAlias>> queues, ProcessorCallback<TAlias, RabbitMQBody> processor, bool recreateQueue = false, ILogger<RabbitMQEventListener<TAlias>>? logger = null)
    {
        _factory = factory;
        _serializer = serializer;
        _queues = queues.Where(x => x.Active == true).ToArray();
        _processor = processor;
        _recreateQueue = recreateQueue;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="waitRetry"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        _logger?.LogDebug("RabbitMQEventListener start");

        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        #region Initialize Queues
        foreach (var entry in _queues)
            _channel.QueueDeclare(entry.Queue, entry.Durable, entry.Exclusive, entry.AutoDelete, null);

        _channel.BasicQos(0, 1, false);
        foreach (var entry in _queues)
        {
            if (_recreateQueue)
                _channel.QueuePurge(entry.Queue);
            _channel.QueueBind(entry.Queue, entry.Exchange, entry.RoutingKey);
        }
        #endregion

        foreach (var entry in _queues)
        {
            var queue = entry.Queue;

            var accountChanges = new EventingBasicConsumer(_channel);
            accountChanges.Received += (sender, e) => ProcessEventAsync(sender, entry, e).Wait();

            _channel.BasicConsume(queue, false, accountChanges);
        }

        return default;
    }

    #region Protected Methods
    /// <summary>
    /// Process event comming from rabbit
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="queue"></param>
    /// <param name="e"></param>
    protected virtual async Task ProcessEventAsync(object? sender, RabbitMQQueueAlias<TAlias> queue, BasicDeliverEventArgs e)
    {
        MessageEnvelop? envelop = null;
        var json = Encoding.UTF8.GetString(e.Body.ToArray());

        // Check if the envelop property is present then deserialize as envelop
        if (e.BasicProperties.Headers?.TryGetValue(Constants.HeaderHasEnvelop, out var hasEnvelop) == true && hasEnvelop.Equals(true))
            envelop = _serializer.Deserialize<MessageEnvelop>(json);
        envelop ??= new MessageEnvelop(json, null);

        var args = new RabbitMQBody(_channel!, e);
        var message = new ProcessMessageArgs<TAlias, RabbitMQBody>(queue, envelop, args, TimeSpan.Zero);
        await _processor(message, CancellationToken.None);
    }
    #endregion
}