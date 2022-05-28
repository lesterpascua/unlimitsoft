using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using SoftUnlimit.Event;
using SoftUnlimit.Web.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Create a backgound process to publish all dispatcher events.
    /// </summary>
    /// <remarks>
    /// The implementation asume the event is store in the <see cref="IUnitOfWork"/> instance. Only store the unique event id and when need to 
    /// publish the background process will read from the <see cref="IRepository{TEntity}"/> and publish in the event bus.
    /// </remarks>
    /// <typeparam name="TEventSourcedRepository"></typeparam>
    /// <typeparam name="TVersionedEventPayload"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    public class QueueEventPublishWorker<TEventSourcedRepository, TVersionedEventPayload, TPayload> : IEventPublishWorker, IDisposable
        where TVersionedEventPayload : VersionedEventPayload<TPayload>
        where TEventSourcedRepository : IEventSourcedRepository<TVersionedEventPayload, TPayload>
    {
        private readonly int _bachSize;
        private readonly bool _enableScheduled;
        private readonly bool _useEnvelop;
        private readonly IServiceScopeFactory _factory;
        private readonly IEventBus _eventBus;
        private readonly MessageType _type;
        private readonly TimeSpan _startDelay, _errorDelay;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentDictionary<Guid, Bucket> _pending;
        private bool _disposed;


        /// <summary>
        /// Initialize instance.
        /// </summary>
        /// <param name="factory">
        /// <list>
        ///     <item>- Resolve TUnitOfWork </item>
        ///     <item>- Resolve <see cref="IRepository{TEventPayload}"/> </item>
        /// </list>
        /// </param>
        /// <param name="eventBus"></param>
        /// <param name="type"></param>
        /// <param name="startDelay">Wait time before start the listener.</param>
        /// <param name="errorDelay">Wait time if some error happened in the bus, 20 second default time.</param>
        /// <param name="bachSize">Amount of pulling event for every iteration. 10 event by default.</param>
        /// <param name="enableScheduled">Enable scheduler feature by software. If false no scheduler feature will added and all will be handler by the queue provider.</param>
        /// <param name="useEnvelop">Use envelop when publish the event</param>
        /// <param name="logger"></param>
        public QueueEventPublishWorker(
            IServiceScopeFactory factory,
            IEventBus eventBus,
            MessageType type,
            TimeSpan? startDelay = null,
            TimeSpan? errorDelay = null,
            int bachSize = 10,
            bool enableScheduled = false,
            bool useEnvelop = true,
            ILogger logger = null)
        {
            _disposed = false;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _type = type;
            _startDelay = startDelay ?? TimeSpan.FromSeconds(5);
            _errorDelay = errorDelay ?? TimeSpan.FromSeconds(20);
            _logger = logger;
            _bachSize = bachSize;
            _enableScheduled = enableScheduled;
            _useEnvelop = useEnvelop;
            _pending = new();
            _cts = new();
        }

        /// <summary>
        /// Job used to publish event
        /// </summary>
        protected Task? Worker { get; set; }
        /// <summary>
        /// Object was disposed
        /// </summary>
        protected bool Disposed => _disposed;
        /// <summary>
        /// Token using to cancelate the background task
        /// </summary>
        protected CancellationTokenSource Cts => _cts;
        /// <summary>
        /// Collection with pending to publish event.
        /// </summary>
        protected ConcurrentDictionary<Guid, Bucket> Pending => _pending;

        /// <inheritdoc />
        public virtual void Dispose()
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
        public async virtual Task StartAsync(bool loadEvent, CancellationToken ct = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (Worker is not null)
                throw new InvalidProgramException("Already initialized");

            // Know issue if all services start at the same time this will be a problem because the event will load multiples times.
            if (loadEvent)
                await LoadEventAsync(ct);

            // Create an independance task to publish all event in the event bus.
            Worker = Task.Run(PublishBackground);

            _logger.LogInformation("EventPublishWorker start");
        }
        /// <inheritdoc />
        public virtual ValueTask RetryPublishAsync(Guid id, CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _pending.TryAdd(id, new Bucket(null, DateTime.UtcNow));
            return ValueTaskExtensions.CompletedTask;
        }
        /// <inheritdoc />
        public virtual ValueTask PublishAsync(IEnumerable<IEvent> events, CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            foreach (var @event in events)
            {
                DateTime? scheduled = null;
                if (@event is IDelayEvent delayEvent)
                    scheduled = delayEvent.Scheduled;

                _pending.TryAdd(@event.Id, new Bucket(scheduled, @event.Created));
            }
            return ValueTaskExtensions.CompletedTask;
        }
        /// <inheritdoc />
        public ValueTask PublishAsync(IEnumerable<PublishEventInfo> events, CancellationToken ct = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            foreach (var @event in events)
                _pending.TryAdd(@event.Id, new Bucket(@event.Scheduled, @event.Created));
            return ValueTaskExtensions.CompletedTask;
        }

        #region Protected Methods
        /// <summary>
        /// Verified if exist any new event to publish
        /// </summary>
        /// <returns></returns>
        protected virtual bool ExistNewEvent()
        {
            if (_cts.Token.IsCancellationRequested)
                return true;
            if (!_pending.IsEmpty)
            {
                if (!_enableScheduled)
                    return true;
                //
                // only if scheduled feature is enable by software
                var now = DateTime.UtcNow;
                return _pending.Any(p => p.Value.Scheduled is null || p.Value.Scheduled.Value <= now);
            }

            return false;
        }
        /// <summary>
        /// Backgound process event to the queue.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task PublishBackground()
        {
            await Task.Delay(_startDelay, _cts.Token);
            while (!_cts.Token.IsCancellationRequested)
            {
                SpinWait.SpinUntil(ExistNewEvent);
                if (_cts.Token.IsCancellationRequested)
                    break;
                _logger?.LogDebug("Start to publish events {Time}", DateTime.UtcNow);

                TVersionedEventPayload lastEvent = null;
                int count = Math.Min(_pending.Count, _bachSize);

                IOrderedEnumerable<KeyValuePair<Guid, Bucket>> orderedPending;
                if (_enableScheduled)
                {
                    var now = DateTime.UtcNow;
                    orderedPending = _pending
                        .Where(p => p.Value.Scheduled is null || p.Value.Scheduled.Value <= now)
                        .OrderBy(k => k.Value);
                }
                else
                    orderedPending = _pending.OrderBy(k => k.Value.Created);

                var buffer = orderedPending.Select(s => s.Key).Take(count).ToArray();
                try
                {
                    using var scope = _factory.CreateScope();
                    var eventSourcedRepository = scope.ServiceProvider.GetRequiredService<TEventSourcedRepository>();

                    var eventsPayload = await eventSourcedRepository.GetEventsAsync(buffer, _cts.Token);
                    foreach (var ePayload in eventsPayload)
                    {
                        if (!ePayload.IsPubliched)
                        {
                            lastEvent = ePayload;
                            await _eventBus.PublishPayloadAsync(lastEvent, _type, _useEnvelop, _cts.Token);
                            await eventSourcedRepository.MarkEventsAsPublishedAsync(lastEvent, _cts.Token);
                        }

                        _pending.TryRemove(ePayload.Id, out var _);
                    }
                }
                catch (Exception ex) when (ex is not TaskCanceledException)
                {
                    _logger?.LogError(ex, "Error publish on {Time} the event: {@Event}.", DateTime.UtcNow, lastEvent);
                    if (!_cts.Token.IsCancellationRequested)
                        await Task.Delay(_errorDelay, _cts.Token);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected virtual async Task LoadEventAsync(CancellationToken ct)
        {
            using var scope = _factory.CreateScope();
            var eventSourcedRepository = scope.ServiceProvider.GetRequiredService<TEventSourcedRepository>();

            int page = 0;
            NonPublishVersionedEventPayload[] nonPublishEvents;
            var pending = new List<NonPublishVersionedEventPayload>();
            do
            {
                var paging = new Paging { Page = page++, PageSize = 1000 };
                nonPublishEvents = await eventSourcedRepository.GetNonPublishedEventsAsync(paging, ct);
                pending.AddRange(nonPublishEvents);
            } while (nonPublishEvents?.Any() == true);

            pending.Sort((x, y) =>
            {
                int compare = 0;
                if (x.Scheduled is null)
                {
                    if (y.Scheduled is not null)
                        compare = DateTime.UtcNow.CompareTo(y.Scheduled.Value);
                }
                else if (y.Scheduled is not null)
                    compare = x.Scheduled.Value.CompareTo(DateTime.UtcNow);

                if (compare != 0)
                    return compare;
                return x.Created.CompareTo(y.Created);
            });

            foreach (var @event in pending)
                _pending.TryAdd(@event.Id, new Bucket(@event.Scheduled, @event.Created));
        }
        #endregion

        #region Nested Classes
        /// <summary>
        /// 
        /// </summary>
        protected class Bucket : IComparable<Bucket>
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="scheduled"></param>
            /// <param name="created"></param>
            public Bucket(DateTime? scheduled, DateTime created)
            {
                Scheduled = scheduled;
                Created = created;
            }

            /// <summary>
            /// 
            /// </summary>
            public DateTime? Scheduled { get; set; } 
            /// <summary>
            /// 
            /// </summary>
            public DateTime Created { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public int CompareTo(Bucket other)
            {
                if (Scheduled is not null && other.Scheduled is not null)
                    return Created.CompareTo(other.Created);
                if (Scheduled is null)
                    return -1;
                return 1;
            }
        }
        #endregion
    }
}
