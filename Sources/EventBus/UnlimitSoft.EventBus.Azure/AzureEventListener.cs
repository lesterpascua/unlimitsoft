using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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
    private readonly int _maxConcurrentCalls, _prefetchCount;
    private readonly ServiceBusAdministrationClient? _adminClient;
    private readonly ILogger<AzureEventListener<TAlias>>? _logger;
    private readonly Func<AzureQueueAlias<TAlias>, Type[]>? _getBindingEvents;

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
    /// <param name="getBindingEvents">Get the events linked to a queue.</param>
    /// <param name="selfManager">Queue will create the necesaries operation to manage hinself</param>
    /// <param name="maxConcurrentCalls">Maximun number of concurrence event to process.</param>
    /// <param name="prefetchCount"></param>
    /// <param name="logger">Logger used to register process data</param>
    public AzureEventListener(
        string endpoint,
        IEnumerable<AzureQueueAlias<TAlias>> queues,
        ProcessorCallback<TAlias, ProcessMessageEventArgs> processor,
        IJsonSerializer serializer,
        Func<AzureQueueAlias<TAlias>, Type[]>? getBindingEvents,
        bool selfManager = true,
        int maxConcurrentCalls = 1,
        int prefetchCount = 0,
        ILogger<AzureEventListener<TAlias>>? logger = null
    )
    {
        _endpoint = endpoint;
        _queues = [.. queues.Where(x => x.Active == true)];
        _processor = processor;
        _serializer = serializer;
        _getBindingEvents = getBindingEvents;
        _maxConcurrentCalls = maxConcurrentCalls;
        _prefetchCount = prefetchCount;
        _logger = logger;

        if (selfManager)
        {
            if (getBindingEvents is null)
                throw new ArgumentNullException(nameof(getBindingEvents), "This value is requiered when is self manager");
            _adminClient = new ServiceBusAdministrationClient(_endpoint);
        }
    }

    /// <summary>
    /// Wait time for retry the event
    /// </summary>
    protected TimeSpan WaitRetry => _waitRetry;

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
        if (_adminClient is not null)
        {
            foreach (var queue in _queues.Where(x => x.Active == true))
            {
                var events = _getBindingEvents(queue);
                foreach (var eventType in events)
                    await TopicHelper.CreateAsync(_adminClient, queue, eventType, ct);
            }
        }

        var group = _queues.GroupBy(k => k.Queue).ToArray();
        _busProcessors = new ServiceBusProcessor[group.Length];
        for (int i = 0; i < group.Length; i++)
        {
            var queue = group[i].First();
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
    protected virtual ServiceBusClient CreateClient() => new(_endpoint);
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

        //if (entry.Subscription is null)
        //    return _client.CreateProcessor(
        //        entry.Queue,
        //        new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, MaxConcurrentCalls = _maxConcurrentCalls }
        //    );

        return _client.CreateProcessor(
            entry.Queue,
            new ServiceBusProcessorOptions { ReceiveMode = ServiceBusReceiveMode.PeekLock, MaxConcurrentCalls = _maxConcurrentCalls, PrefetchCount = _prefetchCount }
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