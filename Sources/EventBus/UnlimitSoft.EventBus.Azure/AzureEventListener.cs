using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.EventBus.Configuration;
using UnlimitSoft.Json;

namespace UnlimitSoft.EventBus.Azure;


/// <summary>
/// Create a bus to listener message from the queue.
/// </summary>
public class AzureEventListener<TAlias> : IEventListener, IAsyncDisposable
    where TAlias : struct, Enum
{
    private readonly string _endpoint;
    private readonly AzureQueueAlias<TAlias>[] _queues;
    private readonly ProcessorCallback<TAlias, ProcessMessageEventArgs> _processor;
    private readonly IJsonSerializer _serializer;
    private readonly int _maxConcurrentCalls;
    private readonly ILogger<AzureEventListener<TAlias>>? _logger;

    private TimeSpan _waitRetry;
    private ServiceBusClient? _client;
    private ServiceBusProcessor[]? _busProcessors;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint">Connection string to azure event bus. Endpoint=sb://my.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=supersecretsharedkey</param>
    /// <param name="queues">Queues where the lister will connect to listener diferent events</param>
    /// <param name="processor">Processor used to process the event, <see cref="ProcessorUtility.Default" /> is used by default</param>
    /// <param name="serializer">Json serializer used to serializer the event</param>
    /// <param name="maxConcurrentCalls">Maximun number of concurrence event to process.</param>
    /// <param name="logger">Logger used to register process data</param>
    public AzureEventListener(
        string endpoint,
        IEnumerable<AzureQueueAlias<TAlias>> queues,
        ProcessorCallback<TAlias, ProcessMessageEventArgs> processor,
        IJsonSerializer serializer,
        int maxConcurrentCalls = 1,
        ILogger<AzureEventListener<TAlias>>? logger = null
    )
    {
        _endpoint = endpoint;
        _queues = queues.Where(x => x.Active == true).ToArray();
        _processor = processor;
        _serializer = serializer;
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

        GC.SuppressFinalize(this);
    }
    /// <inheritdoc />
    public async ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default)
    {
        _logger?.LogDebug("AzureEventListener start");
        if (waitRetry > TimeSpan.FromMinutes(5))
            throw new ArgumentException("Retry time greather the the allowed", nameof(waitRetry));

        _waitRetry = waitRetry;
        _client = CreateClient();
        _busProcessors = new ServiceBusProcessor[_queues.Length];

        for (int i = 0; i < _queues.Length; i++)
        {
            var queue = _queues[i];
            var busProcessor = _busProcessors[i] = CreateProcessorAsync(queue);

            busProcessor.ProcessErrorAsync += args => ProcessErrorAsync(queue.Queue, args);
            busProcessor.ProcessMessageAsync += args => ProcessMessageAsync(queue, args);

            await busProcessor.StartProcessingAsync(ct);
        }
    }

    #region Protected Methods
    /// <summary>
    /// Create instance of the service bus client
    /// </summary>
    /// <returns></returns>
    protected virtual ServiceBusClient CreateClient()
    {
        return new ServiceBusClient(_endpoint);
    }
    /// <summary>
    /// Create processor for every queue
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    protected virtual ServiceBusProcessor CreateProcessorAsync(AzureQueueAlias<TAlias> entry)
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    protected virtual Task ProcessErrorAsync(string queue, ProcessErrorEventArgs arg)
    {
        _logger?.LogError(arg.Exception, "Error from {Queue} in entity: {Entity}", queue, arg.EntityPath);
        return Task.CompletedTask;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    protected virtual async Task ProcessMessageAsync(QueueAlias<TAlias> queue, ProcessMessageEventArgs args)
    {
        MessageEnvelop? envelop = null;
        var json = args.Message.Body.ToString();

        // Check if the envelop property is present then deserialize as envelop
        if (args.Message.ApplicationProperties?.TryGetValue(Constants.HeaderHasEnvelop, out var hasEnvelop) == true && hasEnvelop.Equals(true))
            envelop = _serializer.Deserialize<MessageEnvelop>(json);
        envelop ??= new MessageEnvelop(json, null);

        var message = new ProcessMessageArgs<TAlias, ProcessMessageEventArgs>(queue, envelop, args, _waitRetry);
        await _processor(message, args.CancellationToken);
    }
    #endregion
}



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
