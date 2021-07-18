using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Create a backgound process to publish all dispatcher events
    /// </summary>
    /// <typeparam name="TUnitOfWork">Register Unit of Work interface. Late used by <see cref="IServiceProvider"/> to update event state in database.</typeparam>
    /// <typeparam name="TVersionedEventRepository"></typeparam>
    /// <typeparam name="TVersionedEventPayload"></typeparam>
    /// <typeparam name="TPayload"></typeparam>
    public class QueueEventPublishWorker<TUnitOfWork, TVersionedEventRepository, TVersionedEventPayload, TPayload> : IEventPublishWorker, IAsyncDisposable
        where TUnitOfWork : IUnitOfWork
        where TVersionedEventPayload : VersionedEventPayload<TPayload>
        where TVersionedEventRepository : IRepository<TVersionedEventPayload>
    {
        private readonly int _bachSize;
        private readonly IServiceProvider _provider;
        private readonly IEventBus _eventBus;
        private readonly MessageType _type;
        private readonly TimeSpan _checkTime;
        private readonly ILogger _logger;
        private Task _backgoundWorker;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentQueue<Guid> _queue;
        private bool _disposed;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="eventBus"></param>
        /// <param name="type"></param>
        /// <param name="checkTime"></param>
        /// <param name="logger"></param>
        /// <param name="bachSize"></param>
        public QueueEventPublishWorker(
            IServiceProvider provider,
            IEventBus eventBus,
            MessageType type,
            TimeSpan? checkTime = null,
            ILogger logger = null,
            int bachSize = 10)
        {
            _disposed = false;
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _type = type;
            _checkTime = checkTime ?? TimeSpan.FromSeconds(5);
            _logger = logger;
            _bachSize = bachSize;
            _queue = new ConcurrentQueue<Guid>();

            _backgoundWorker = null;
            _cts = new CancellationTokenSource();
        }

        /// <inheritdoc />
        public void Init()
        {
            using var scope = _provider.CreateScope();
            var eventPayloadRepository = scope.ServiceProvider.GetService<TVersionedEventRepository>();
            var notPublishEvents = eventPayloadRepository
                .FindAll()
                .Where(p => !p.IsPubliched)
                .OrderBy(k => k.Created)
                .Select(s => s.Id)
                .ToArray();
            foreach (var eventId in notPublishEvents)
                _queue.Enqueue(eventId);

            _backgoundWorker = Task.Run(PublishBackground, _cts.Token);
        }
        /// <summary>
        /// Release processor
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            if (_backgoundWorker != null)
                await _backgoundWorker;
            _disposed = true;
        }
        /// <inheritdoc />
        public void Publish(IEnumerable<IEvent> events)
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().FullName);

            foreach (var @event in events)
                _queue.Enqueue(@event.Id);
        }

        /// <summary>
        /// Backgound process event to the queue.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task PublishBackground()
        {
            await Task.Delay(_checkTime);
            while (!_cts.Token.IsCancellationRequested)
            {
                SpinWait.SpinUntil(() => !_queue.IsEmpty || _cts.Token.IsCancellationRequested);
                if (_cts.Token.IsCancellationRequested)
                    break;
                _logger?.LogDebug("Start to publish events {Time}", DateTime.UtcNow);

                int count = Math.Min(_queue.Count, _bachSize);
                var buffer = _queue.Take(count).ToArray();
                try
                {
                    using var scope = _provider.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetService<TUnitOfWork>();
                    var eventPayloadRepository = scope.ServiceProvider.GetService<TVersionedEventRepository>();

                    var eventsPayload = await eventPayloadRepository
                        .FindAll()
                        .Where(p => buffer.Contains(p.Id))
                        .OrderBy(k => k.Created)
                        .ToArrayAsync();
                    foreach (var eventPayload in eventsPayload)
                    {
                        await _eventBus.PublishEventPayloadAsync(eventPayload, _type);
                        eventPayload.MarkEventAsPublished();
                    }
                    await unitOfWork.SaveChangesAsync();
                    //
                    // dequeue all publish events.
                    for (int i = 0; i < count; i++)
                        _queue.TryDequeue(out Guid eventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publish event: {Time}.", DateTime.UtcNow);
                }
            }
        }
    }
}
