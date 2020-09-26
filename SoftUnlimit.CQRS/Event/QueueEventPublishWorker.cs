using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TUnitOfWork"></typeparam>
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
        private readonly ILogger<IEventPublishWorker> _logger;
        private readonly Task _backgoundWorker;
        private readonly CancellationTokenSource _cts;
        private readonly ConcurrentQueue<Guid> _queue;

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="eventBus"></param>
        /// <param name="logger"></param>
        /// <param name="bachSize"></param>
        public QueueEventPublishWorker(IServiceProvider provider, IEventBus eventBus, ILogger<IEventPublishWorker> logger = null, int bachSize = 10)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = logger;
            _bachSize = bachSize;
            _queue = new ConcurrentQueue<Guid>();

            _cts = new CancellationTokenSource();
            _backgoundWorker = Task.Run(PublishBackground, _cts.Token);
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
                .Select(s => s.CreatorId)
                .ToArray();
            foreach (var eventId in notPublishEvents)
                _queue.Enqueue(eventId);
        }
        /// <summary>
        /// Release processor
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            await _backgoundWorker;
        }
        /// <inheritdoc />
        public void Publish(IEnumerable<IEvent> events)
        {
            foreach (var @event in events)
                _queue.Enqueue(@event.Id);
        }

        /// <summary>
        /// Backgound process event to the queue.
        /// </summary>
        /// <returns></returns>
        private async Task PublishBackground()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                SpinWait.SpinUntil(() => !_queue.IsEmpty || _cts.Token.IsCancellationRequested);
                if (_cts.Token.IsCancellationRequested)
                    break;
                await Task.Delay(TimeSpan.FromSeconds(1));
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
                        .Where(p => buffer.Contains(p.CreatorId))
                        .OrderBy(k => k.Created)
                        .ToArrayAsync();
                    foreach (var entity in eventsPayload)
                    {
                        await _eventBus.PublishEventPayloadAsync(entity);
                        entity.MarkEventAsPublished();
                    }
                    await unitOfWork.SaveChangesAsync();
                    //
                    // dequeue all publish events.
                    for (int i = 0; i < count; i++)
                        _queue.TryDequeue(out Guid eventId);
                } catch (Exception ex)
                {
                    _logger.LogError(ex, "Error publish event: {Time}.", DateTime.UtcNow);
                }
            }
        }
    }
}
