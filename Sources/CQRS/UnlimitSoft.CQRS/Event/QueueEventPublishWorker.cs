using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.Data;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Model;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Create a backgound process to publish all dispatcher events.
/// </summary>
/// <remarks>
/// The implementation asume the event is store in the <see cref="IUnitOfWork"/> instance. Only store the unique event id and when need to 
/// publish the background process will read from the <see cref="IRepository{TEntity}"/> and publish in the event bus.
/// </remarks>
/// <typeparam name="TEventSourcedRepository"></typeparam>
/// <typeparam name="TEventPayload"></typeparam>
public class QueueEventPublishWorker<TEventSourcedRepository, TEventPayload> : IEventPublishWorker
    where TEventPayload : EventPayload
    where TEventSourcedRepository : IEventRepository<TEventPayload>
{
    /// <summary>
    /// Amount of event load per attempt
    /// </summary>
    protected readonly int _bachSize;
    /// <summary>
    /// Create the event into an envelop with event information
    /// </summary>
    protected readonly bool _useEnvelop;
    /// <summary>
    /// Delay time between iterations. To avoid extensive CPU consumtion
    /// </summary>
    protected readonly int _iterationDelay;

    /// <summary>
    /// Clock for the system
    /// </summary>
    protected readonly ISysClock _clock;

    /// <summary>
    /// Factory to create scope
    /// </summary>
    protected readonly IServiceScopeFactory _factory;
    /// <summary>
    /// Event bus where the event will be publish
    /// </summary>
    protected readonly IEventBus _eventBus;
    /// <summary>
    /// Middleware used to publish event usefull to include log information or other
    /// </summary>
    protected readonly Func<TEventPayload, Func<Task>, CancellationToken, Task>? _middleware;
    /// <summary>
    /// Delay after start and error 
    /// </summary>
    protected readonly TimeSpan _startDelay, _errorDelay;
    /// <summary>
    /// Token using to cancelate the background task
    /// </summary>
    protected readonly CancellationTokenSource _cts;
    /// <summary>
    /// Collection with pending to publish event ordering by priority
    /// </summary>
    protected readonly SortedList<PublishEventInfo, bool> _pending;
    /// <summary>
    /// Lock object to make collection safe
    /// </summary>
    protected readonly SemaphoreSlim _lock;
    /// <summary>
    /// Logger used to trace information
    /// </summary>
    protected readonly ILogger<QueueEventPublishWorker<TEventSourcedRepository, TEventPayload>>? _logger;

    private bool _disposed;


    /// <summary>
    /// Initialize instance.
    /// </summary>
    /// <param name="clock"></param>
    /// <param name="factory">
    /// <list>
    ///     <item>- Resolve TUnitOfWork </item>
    ///     <item>- Resolve <see cref="IRepository{TEventPayload}"/> </item>
    /// </list>
    /// </param>
    /// <param name="eventBus"></param>
    /// <param name="middleware">Is is not null will call this function instead the action funcion, delegating the responsabilite to call the action for the middleware</param>
    /// <param name="startDelay">Wait time before start the listener.</param>
    /// <param name="errorDelay">Wait time if some error happened in the bus, 20 second default time.</param>
    /// <param name="bachSize">Amount of pulling event for every iteration. 10 event by default.</param>
    /// <param name="useEnvelop">Use envelop when publish the event</param>
    /// <param name="iterationDelay">Delay time between iterations. To avoid extensive CPU consumtion.</param>
    /// <param name="logger"></param>
    public QueueEventPublishWorker(
        ISysClock clock,
        IServiceScopeFactory factory,
        IEventBus eventBus,
        Func<TEventPayload, Func<Task>, CancellationToken, Task>? middleware = null,
        TimeSpan? startDelay = null,
        TimeSpan? errorDelay = null,
        int bachSize = 10,
        bool useEnvelop = true,
        int iterationDelay = 0,
        ILogger<QueueEventPublishWorker<TEventSourcedRepository, TEventPayload>>? logger = null
    )
    {
        _disposed = false;
        _clock = clock;
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

        _middleware = middleware;
        _startDelay = startDelay ?? TimeSpan.FromSeconds(5);
        _errorDelay = errorDelay ?? TimeSpan.FromSeconds(20);
        _logger = logger;
        _bachSize = bachSize;
        _useEnvelop = useEnvelop;
        _iterationDelay = iterationDelay;
        _pending = [];
        _lock = new(1, 1);
        _cts = new();
    }

    /// <inheritdoc />
    public int Pending => _pending.Count;

    /// <summary>
    /// Job used to publish event
    /// </summary>
    protected Task? Worker { get; set; }
    /// <summary>
    /// Object was disposed
    /// </summary>
    protected bool Disposed => _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        if (Worker is not null)
        {
            try
            {
                Worker.Wait();
            }
            catch (TaskCanceledException) { }
            catch (AggregateException e) when (e.InnerException is TaskCanceledException) { }
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task RetryPublishAsync(Guid id, CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

        var key = new PublishEventInfo(id, _clock.UtcNow, null);
        await _lock.WaitAsync(ct);
        try
        {
            _pending.Add(key, true);
        }
        finally
        {
            _lock.Release();
        }
    }
    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

        await _lock.WaitAsync(ct);
        try
        {
            foreach (var @event in events)
            {
                DateTime? scheduled = null;
                if (@event is IDelayEvent delayEvent)
                    scheduled = delayEvent.Scheduled;

                var key = new PublishEventInfo(@event.Id, @event.Created, scheduled);
                _pending.Add(key, true);
            }
        }
        finally
        {
            _lock.Release();
        }
    }
    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<PublishEventInfo> events, CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

        await _lock.WaitAsync(ct);
        try
        {
            foreach (var @event in events)
            {
                var key = new PublishEventInfo(@event.Id, @event.Created, @event.Scheduled);
                _pending.Add(key, true);
            }
        }
        finally
        {
            _lock.Release();
        }
    }
    /// <inheritdoc />
    public async Task StartAsync(bool loadEvent, int bachSize = 1000, CancellationToken ct = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
        if (Worker is not null)
            throw new InvalidProgramException("Already initialized");

        // Know issue if all services start at the same time this will be a problem because the event will load multiples times.
        if (loadEvent)
            await LoadEventAsync(bachSize, ct);

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods
        // Create an independance task to publish all event in the event bus.
        Worker = Task.Run(PublishBackground);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods

        _logger?.LogInformation("EventPublishWorker start");
    }

    #region Protected Methods
    /// <summary>
    /// Verified if exist any new event to publish
    /// </summary>
    /// <returns></returns>
    protected virtual bool ExistNewEvent()
    {
        if (_iterationDelay != 0)
        {
            try
            {
                Task.Delay(_iterationDelay).Wait(_cts.Token);
            }
            catch { }       // Ignore exception
        }

        if (_cts.Token.IsCancellationRequested)
            return true;

        if (_pending.Count == 0)
            return false;

        var first = _pending.Keys[0];
        return first.Scheduled is null || first.Scheduled.Value <= _clock.UtcNow;
    }
    /// <summary>
    /// Backgound process event to the queue.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task PublishBackground()
    {
        TEventPayload? lastEvent = null;
        await Task.Delay(_startDelay, _cts.Token);

        while (!_cts.Token.IsCancellationRequested)
        {
            SpinWait.SpinUntil(ExistNewEvent);
            if (_cts.Token.IsCancellationRequested)
                break;

            // Dispatch event throws the bus
            _logger?.LogDebug("Start to publish events");

            int bachSize = Math.Min(_pending.Count, _bachSize);

            try
            {
                int index = 0;
                var now = _clock.UtcNow;
                var ids = new Dictionary<Guid, PublishEventInfo>();

                await _lock.WaitAsync(_cts.Token);
                try
                {
                    while (index < bachSize)
                    {
                        var key = _pending.Keys[index];
                        if (key.Scheduled is not null && now < key.Scheduled.Value)
                            break;
                        ids.Add(key.Id, key);
                        index++;
                    }
                }
                finally 
                { 
                    _lock.Release();
                }
                if (ids.Count == 0)
                    return;

                using var scope = _factory.CreateScope();
                var eventSourcedRepository = scope.ServiceProvider.GetRequiredService<TEventSourcedRepository>();

                var eventsPayload = await eventSourcedRepository.GetEventsAsync(ids.Keys.ToArray(), _cts.Token);
                foreach (var payload in eventsPayload)
                {
                    var aux = await TryToPublishAsync(eventSourcedRepository, payload);
                    if (aux is not null)
                        lastEvent = aux;


                    await _lock.WaitAsync(_cts.Token);
                    try
                    {
                        _pending.Remove(ids[payload.Id]);
                    }
                    finally
                    {
                        _lock.Release();
                    }
                }

                _logger?.LogInformation("Publish {Try} events and found {Found}", ids.Count, eventsPayload.Count);
            }
            catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
            {
                _logger?.LogError(
                    ex, 
                    "Error publish with correlation: {CorrelationId} and {@Event}.", lastEvent?.CorrelationId, lastEvent
                );
                await Task.Delay(_errorDelay, _cts.Token);
            }
        }
    }
    /// <summary>
    /// Publish payload and mark as publish
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    protected async Task PublishPayloadAsync(TEventSourcedRepository repository, TEventPayload payload)
    {
        await _eventBus.PublishPayloadAsync(payload, _useEnvelop, _cts.Token);
        await repository.MarkEventsAsPublishedAsync(payload, _cts.Token);
    }
    /// <summary>
    /// Try to publish the event in the queue
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    protected virtual async Task<TEventPayload?> TryToPublishAsync(TEventSourcedRepository repository, TEventPayload payload)
    {
        if (payload.IsPubliched)
            return null;

        if (_middleware is not null)
        {
            await _middleware(payload, () => PublishPayloadAsync(repository, payload), _cts.Token);
            return payload;
        }

        await PublishPayloadAsync(repository, payload);
        return payload;
    }
    /// <summary>
    /// Load all event from the database.
    /// </summary>
    /// <param name="bashSize"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    protected virtual async Task LoadEventAsync(int bashSize, CancellationToken ct)
    {
        using var scope = _factory.CreateScope();
        var eventSourcedRepository = scope.ServiceProvider.GetRequiredService<TEventSourcedRepository>();

        List<NonPublishEventPayload>? nonPublishEvents;
        var pending = new List<NonPublishEventPayload>();
        var paging = new Paging { Page = 0, PageSize = bashSize };
        do
        {
            try
            {
                nonPublishEvents = await eventSourcedRepository.GetNonPublishedEventsAsync(paging, ct);
                _logger?.LogInformation("Event loaded: {Count}", paging.Page * paging.PageSize + nonPublishEvents.Count);

                pending.AddRange(nonPublishEvents);
                paging.Page++;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error retrieving pending event from database, {@Page}", paging);
                nonPublishEvents = null;
            }
        } while (nonPublishEvents is null || nonPublishEvents.Count == bashSize);

        await _lock.WaitAsync(ct);
        try
        {
            // Insert all
            foreach (var @event in pending)
                _pending.Add(new PublishEventInfo(@event.Id, @event.Created, @event.Scheduled), true);
        }
        finally
        {
            _lock.Release();
        }
    }
    #endregion
}
