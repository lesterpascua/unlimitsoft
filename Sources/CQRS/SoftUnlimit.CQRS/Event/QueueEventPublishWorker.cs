using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.Data;
using SoftUnlimit.Event;
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
    /// <typeparam name="TUnitOfWork">Register Unit of Work interface. Late used by <see cref="IServiceProvider"/> to update event state in database.</typeparam>
    /// <typeparam name="TEventRepository"></typeparam>
    /// <typeparam name="TEventPayload"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    public class QueueEventPublishWorker<TUnitOfWork, TEventRepository, TEventPayload, TPayload> : IEventPublishWorker, IAsyncDisposable
        where TUnitOfWork : IUnitOfWork
        where TEventPayload : EventPayload<TPayload>
        where TEventRepository : IRepository<TEventPayload>
    {
        private readonly int _bachSize;
        private readonly IServiceScopeFactory _factory;
        private readonly IEventBus _eventBus;
        private readonly MessageType _type;
        private readonly TimeSpan _startDelay, _errorDelay;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentQueue<Guid> _queue;
        private bool _disposed;
        private Task _backgoundWorker;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="eventBus"></param>
        /// <param name="type"></param>
        /// <param name="startDelay">Wait time before start the listener.</param>
        /// <param name="errorDelay">Wait time if some error happened in the bus, 20 second default time.</param>
        /// <param name="bachSize">Amount of pulling event for every iteration. 10 event by default.</param>
        /// <param name="logger"></param>
        public QueueEventPublishWorker(
            IServiceScopeFactory factory,
            IEventBus eventBus,
            MessageType type,
            TimeSpan? startDelay = null,
            TimeSpan? errorDelay = null,
            int bachSize = 10,
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
            _backgoundWorker = null;
            _queue = new ConcurrentQueue<Guid>();
            _cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Queue of pending event identifier.
        /// </summary>
        protected ConcurrentQueue<Guid> Queue => _queue;

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            if (_backgoundWorker != null)
                await _backgoundWorker;
            _disposed = true;
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (_backgoundWorker != null)
                throw new InvalidProgramException("Already initialized");

            using var scope = _factory.CreateScope();
            var eventPayloadRepository = scope.ServiceProvider.GetService<TEventRepository>();
            var nonPublishEvents = await Task.Run(
                () => eventPayloadRepository
                        .FindAll()
                        .Where(p => !p.IsPubliched)
                        .OrderBy(k => k.Created)
                        .Select(s => s.Id)
                        .ToArray(),
                ct
            );

            foreach (var eventId in nonPublishEvents)
                _queue.Enqueue(eventId);

#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods that take one
            // Create an independance task to publish all event in the event bus.
            _backgoundWorker = Task.Run(PublishBackground);
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods that take one
        }
        /// <inheritdoc />
        public ValueTask RetryPublishAsync(Guid id, CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            _queue.Enqueue(id);
            return ValueTask.CompletedTask;
        }
        /// <inheritdoc />
        public ValueTask PublishAsync(IEnumerable<IEvent> events, CancellationToken ct)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);

            foreach (var @event in events)
                _queue.Enqueue(@event.Id);
            return ValueTask.CompletedTask;
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
                SpinWait.SpinUntil(() => !_queue.IsEmpty || _cts.Token.IsCancellationRequested);
                if (_cts.Token.IsCancellationRequested)
                    break;
                _logger?.LogDebug("Start to publish events {Time}", DateTime.UtcNow);

                TEventPayload lastEvent = null;
                int count = Math.Min(_queue.Count, _bachSize);
                var buffer = _queue.Take(count).ToArray();
                try
                {
                    using var scope = _factory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetService<TUnitOfWork>();
                    var eventPayloadRepository = scope.ServiceProvider.GetService<TEventRepository>();

                    var eventsPayload = await Task.Run(
                        () => eventPayloadRepository
                                .FindAll()
                                .Where(p => buffer.Contains(p.Id) && !p.IsPubliched)
                                .OrderBy(k => k.Created)
                                .ToArray(), 
                        _cts.Token
                    );
                    foreach (var ePayload in eventsPayload)
                    {
                        lastEvent = ePayload;
                        await _eventBus.PublishPayloadAsync(lastEvent, _type, _cts.Token);

                        lastEvent.MarkEventAsPublished();
                        await unitOfWork.SaveChangesAsync(_cts.Token);
                    }
                    //
                    // dequeue all publish events.
                    for (var i = 0; i < count; i++)
                        _queue.TryDequeue(out Guid eventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publish on {Time} the event: {@Event}.", DateTime.UtcNow, lastEvent);
                    await Task.Delay(_errorDelay, _cts.Token);
                }
            }
        }
    }
}
