using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.EventBus.Azure.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus.Administration;

namespace UnlimitSoft.EventBus.Azure;


/// <summary>
/// Create a bus to listener message from the queue.
/// </summary>
public class AzureEventListener<TAlias> : IEventListener, IAsyncDisposable
    where TAlias : Enum
{
    private readonly string _endpoint;
    private readonly QueueAlias<TAlias>[] _queues;
    private readonly ProcessorCallback _processor;
    private readonly int _maxConcurrentCalls;
    private readonly ILogger? _logger;

    private TimeSpan _waitRetry;
    private ServiceBusClient? _client;
    private ServiceBusProcessor[]? _busProcessors;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="queues"></param>
    /// <param name="processor">Processor used to process the event <see cref="ProcessorUtility.Default{TEvent}(IEventDispatcher, CQRS.Event.Json.IEventNameResolver, MessageEnvelop, ServiceBusReceivedMessage, Action{TEvent}, Func{Exception, TEvent, MessageEnvelop, CancellationToken, Task}, ILogger, CancellationToken)"/></param>
    /// <param name="maxConcurrentCalls"></param>
    /// <param name="logger"></param>
    public AzureEventListener(
        string endpoint,
        IEnumerable<QueueAlias<TAlias>> queues,
        ProcessorCallback processor,
        int maxConcurrentCalls = 1,
        ILogger? logger = null
    )
    {
        _endpoint = endpoint;
        _queues = queues.ToArray();
        _processor = processor;
        _maxConcurrentCalls = maxConcurrentCalls;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_busProcessors is null || _client is null)
            return;

        for (int i = 0; i < _busProcessors.Length; i++)
            await _busProcessors[i].StopProcessingAsync();
        await _client.DisposeAsync();
    }
    /// <inheritdoc />
    public async ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        _logger?.LogDebug("AzureEventListener start");
        if (waitRetry > TimeSpan.FromMinutes(5))
            throw new ArgumentException("Retry time greather the the allowed", nameof(waitRetry));

        _waitRetry = waitRetry;
        _client = new ServiceBusClient(_endpoint);
        _busProcessors = new ServiceBusProcessor[_queues.Length];

        for (int i = 0; i < _queues.Length; i++)
        {
            var entry = _queues[i];
            var busProcessor = _busProcessors[i] = CreateProcessorAsync(entry);

            busProcessor.ProcessErrorAsync += args => ProcessErrorAsync(entry.Queue, args);
            busProcessor.ProcessMessageAsync += args => ProcessMessageAsync(entry.Queue, args);

            await busProcessor.StartProcessingAsync(ct);
        }
    }

    #region Private Methods
    //private Task CreateIfNotExistAsync(QueueAlias<TAlias> entry, CancellationToken ct)
    //{
    //    return Task.CompletedTask;
    //    //var administrationClient = new ServiceBusAdministrationClient(_endpoint);
    //    //var topics = administrationClient.GetTopicsAsync();
    //    //var asyncEnumerable = topics.AsPages();

    //    //var topics = _queues.Where(p => p.Subscription);

    //    //var enumerator = asyncEnumerable.GetAsyncEnumerator();
    //    //while (await enumerator.MoveNextAsync())
    //    //{
    //    //    var topic = enumerator.Current;
    //    //    var t = topic.Values.FirstOrDefault(p => p.Name == );

    //    //    Console.WriteLine(t);
    //    //}
    //}
    private ServiceBusProcessor CreateProcessorAsync(QueueAlias<TAlias> entry)
    {
        if (_busProcessors is null || _client is null)
            throw new InvalidOperationException("Call ListenAsync first");

        //await CreateIfNotExistAsync(entry, ct);

        if (entry.Subscription is null)
            return _client.CreateProcessor(
                entry.Queue,
                new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, MaxConcurrentCalls = _maxConcurrentCalls }
            );
        return _client.CreateProcessor(
            entry.Queue,
            entry.Subscription,
            new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, MaxConcurrentCalls = _maxConcurrentCalls }
        );
    }

    private Task ProcessErrorAsync(string queue, ProcessErrorEventArgs arg)
    {
        _logger?.LogError(arg.Exception, "Error from {Queue} in entity: {Entity}", queue, arg.EntityPath);
        return Task.CompletedTask;
    }
    private async Task ProcessMessageAsync(string queue, ProcessMessageEventArgs args)
    {
        var envelop = args.Message.Body?.ToObjectFromJson<MessageEnvelop>();
        if (envelop is null)
        {
            _logger?.LogWarning("Invalid evelop for {MessageId}", args.Message.MessageId);
            return;
        }

        var message = new ProcessMessageArgs(queue, envelop, args, _waitRetry, _logger);
        await _processor(message, args.CancellationToken);
    }
    #endregion
}