using DotNetMQ.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.EventBus.Configuration;
using UnlimitSoft.Json;

namespace UnlimitSoft.EventBus.DotNetMQ;


/// <summary>
/// Create a bus to listener message from the queue.
/// </summary>
public class MemoryEventListener<TAlias> : IEventListener, IAsyncDisposable
    where TAlias : struct, Enum
{
    private readonly QueueAlias<TAlias>[] _queues;
    private readonly ProcessorCallback<TAlias, IIncomingMessage> _processor;
    private readonly IJsonSerializer _serializer;
    private readonly ILogger<MemoryEventListener<TAlias>>? _logger;

    private TimeSpan _waitRetry;
    private MDSClient[]? _clients;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queues">Queues where the lister will connect to listener diferent events</param>
    /// <param name="processor">Processor used to process the event</param>
    /// <param name="serializer">Json serializer used to serializer the event</param>
    /// <param name="logger">Logger used to register process data</param>
    public MemoryEventListener(
        IEnumerable<QueueAlias<TAlias>> queues,
        ProcessorCallback<TAlias, IIncomingMessage> processor,
        IJsonSerializer serializer,
        ILogger<MemoryEventListener<TAlias>>? logger = null
    )
    {
        _processor = processor;
        _queues = queues.Where(x => x.Active == true).ToArray();
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        if (_clients is not null)
            for (int i = 0; i < _clients.Length; i++)
                _clients[i].Dispose();
        GC.SuppressFinalize(this);

#if NETSTANDARD2_0
        return new ValueTask(Task.CompletedTask);
#else
        return ValueTask.CompletedTask;
#endif
    }
    /// <inheritdoc />
    public ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        _logger?.LogDebug("AzureEventListener start");
        if (waitRetry > TimeSpan.FromMinutes(5))
            throw new ArgumentException("Retry time greather the the allowed", nameof(waitRetry));

        _waitRetry = waitRetry;
        _clients = new MDSClient[_queues.Length];
        for (int i = 0; i < _queues.Length; i++)
        {
            var queue = _queues[i];
            var client = _clients[i] = CreateClient(queue);
            client.MessageReceived += (sender, e) => ProcessMessageAsync(queue, sender, e).Wait();

            client.Connect();
        }

#if NETSTANDARD2_0
        return new ValueTask(Task.CompletedTask);
#else
        return ValueTask.CompletedTask;
#endif
    }

    /// <summary>
    /// Create client 
    /// </summary>
    /// <param name="queue"></param>
    /// <returns></returns>
    protected virtual MDSClient CreateClient(QueueAlias<TAlias> queue)
    {
        var client = new MDSClient(queue.Queue);
        return client;
    }

    #region Private Methods
    private async Task ProcessMessageAsync(QueueAlias<TAlias> queue, object sender, MessageReceivedEventArgs args)
    {
        var json = Encoding.UTF8.GetString(args.Message.MessageData);
        var envelop = _serializer.Deserialize<MessageEnvelop>(json);
        if (envelop is null)
        {
            _logger?.LogWarning("Invalid evelop for {MessageId}", args.Message.MessageId);
            return;
        }

        var message = new ProcessMessageArgs<TAlias, IIncomingMessage>(queue, envelop, args.Message, _waitRetry);
        await _processor(message, default);
    }
    #endregion
}