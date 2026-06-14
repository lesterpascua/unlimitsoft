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
    private readonly Func<string, CancellationToken, Task>? _exchangeFactory;
    private readonly ILogger<RabbitMQEventListener<TAlias>>? _logger;

    private IChannel? _channel;
    private IConnection? _connection;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="serializer"></param>
    /// <param name="queues"></param>
    /// <param name="processor"></param>
    /// <param name="recreateQueue"></param>
    /// <param name="exchangeFactory"></param>
    /// <param name="logger"></param>
    public RabbitMQEventListener(string endpoint, IJsonSerializer serializer, 
        IEnumerable<RabbitMQQueueAlias<TAlias>> queues, 
        ProcessorCallback<TAlias, RabbitMQBody> processor, 
        bool recreateQueue = false,
        Func<string, CancellationToken, Task>? exchangeFactory = null,
        ILogger<RabbitMQEventListener<TAlias>>? logger = null)
        : this(new ConnectionFactory { Uri = new Uri(endpoint) }, serializer, queues, processor, recreateQueue, exchangeFactory, logger)
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
    /// <param name="exchangeFactory"></param>
    /// <param name="logger"></param>
    public RabbitMQEventListener(
        IConnectionFactory factory, 
        IJsonSerializer serializer, 
        IEnumerable<RabbitMQQueueAlias<TAlias>> queues, 
        ProcessorCallback<TAlias, RabbitMQBody> processor, 
        bool recreateQueue = false,
        Func<string, CancellationToken, Task>? exchangeFactory = null,
        ILogger<RabbitMQEventListener<TAlias>>? logger = null)
    {
        _factory = factory;
        _serializer = serializer;
        _queues = queues.Where(x => x.Active == true).ToArray();
        _processor = processor;
        _recreateQueue = recreateQueue;
        _exchangeFactory = exchangeFactory;
        _logger = logger;

        _exchangeFactory ??= (exchange, ct) => _channel!.ExchangeDeclareAsync(exchange, ExchangeType.Direct, true, false, cancellationToken: ct);
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
    public async ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        _logger?.LogDebug("RabbitMQEventListener start");

        _connection = await _factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        #region Initialize Queues
        foreach (var entry in _queues)
            await _channel.QueueDeclareAsync(entry.Queue, entry.Durable, entry.Exclusive, entry.AutoDelete, null, cancellationToken: ct);

        await _channel.BasicQosAsync(0, 1, false, ct);
        foreach (var queue in _queues)
        {
            if (_recreateQueue)
                await _channel.QueuePurgeAsync(queue.Queue, ct);

            if (_exchangeFactory is not null)
                await _exchangeFactory(queue.Exchange, ct);

            await _channel.ExchangeDeclareAsync(queue.Exchange, ExchangeType.Direct, true, false, cancellationToken: ct);
            foreach (var rk in queue.RoutingKey)
                await _channel.QueueBindAsync(queue.Queue, queue.Exchange, rk, cancellationToken: ct);
        }
        #endregion

        var comparer = new QueueComparer();
        foreach (var entry in _queues.Distinct(comparer))
        {
            var queue = entry.Queue;

            var accountChanges = new AsyncEventingBasicConsumer(_channel);
            accountChanges.ReceivedAsync += (sender, e) => ProcessEventAsync(sender, entry, e);

            await _channel.BasicConsumeAsync(queue, false, accountChanges, ct);
        }
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

    #region Nested Classes
    private sealed class QueueComparer : IEqualityComparer<RabbitMQQueueAlias<TAlias>>
    {
        /// <inheritdoc />
        public int GetHashCode(RabbitMQQueueAlias<TAlias> obj) => obj.Alias.GetHashCode();
        /// <inheritdoc/>
        public bool Equals(RabbitMQQueueAlias<TAlias>? x, RabbitMQQueueAlias<TAlias>? y) => Equals(x?.Queue, y?.Queue);
    }
    #endregion
}