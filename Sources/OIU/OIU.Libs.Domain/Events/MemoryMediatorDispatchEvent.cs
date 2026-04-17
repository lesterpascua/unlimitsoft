using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.Event;
using OIU.Libs.Events;
using System.Net;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Client;

namespace OIU.Libs.Domain.Events;


/// <inheritdoc />
public sealed class MemoryMediatorDispatchEvent : MediatorDispatchEvent<OIUEventPayload>
{
    private readonly IJsonSerializer _serializer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="serializer"></param>
    public MemoryMediatorDispatchEvent(IServiceProvider provider, IJsonSerializer serializer)
        : base(provider, true)
    {
        _serializer = serializer;
    }

    /// <inheritdoc />
    protected override OIUEventPayload Create(IEvent @event)
    {
        if (@event is not IEventX e)
            throw new NotSupportedException($"Only allow {nameof(IEventX)}");

        return new OIUEventPayload(e, _serializer);
    }

    /// <inheritdoc />
    protected override IEventRepository<OIUEventPayload>? EventRepository => Provider.GetService<IOIUEventRepository>();

    /// <inheritdoc />
    public override async Task DispatchEventsAsync(IEnumerable<IEvent> events, bool forceSave, bool stopCreation, CancellationToken ct)
    {
        var eventRepository = EventRepository;
        var eventsPayload = (eventRepository is not null && !stopCreation) ? new List<OIUEventPayload>() : null;

        foreach (var @event in events)
        {
            var payload = Create(@event);
            eventsPayload?.Add(payload);

            if (EventDispatcher is null || (!@event.IsDomainEvent && !DirectlyDispatchNotDomainEvents))
                continue;

            var (response, error) = await EventDispatcher.PublishAsync(Provider, @event, ct);
            if (error is null || error.Code == HttpStatusCode.NotFound)
                continue;

            var msg = "Error when executed events";
            throw new AggregateException(msg, error.GetBody<Exception>() ?? throw new InvalidOperationException(msg));
        }
        if (eventsPayload is not null && eventRepository is not null)
            await eventRepository.CreateAsync(eventsPayload, forceSave, ct);
    }

    #region Private Methods
    internal sealed class InternalEventPublishWorker : IEventPublishWorker
    {
        private readonly IServiceProvider _provider;
        private readonly IEventDispatcher _dispatcher;

        public InternalEventPublishWorker(IServiceProvider provider, IEventDispatcher dispatcher)
        {
            _provider = provider;
            _dispatcher = dispatcher;
        }

        /// <inheritdoc />
        public int Pending => 0;

        /// <inheritdoc />
        public void Dispose() { }
        /// <inheritdoc />
        public Task StartAsync(bool loadEvent, int bachSize = 1000, CancellationToken ct = default) => Task.CompletedTask;

        /// <inheritdoc />
        public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken ct = default)
        {
            using var scope = _provider.CreateScope();
            var provider = scope.ServiceProvider;
            foreach (var @event in events)
                await _dispatcher.DispatchAsync(provider, @event, ct);
        }
        /// <inheritdoc />
        public Task RetryPublishAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        /// <inheritdoc />
        public Task PublishAsync(IEnumerable<PublishEventInfo> events, CancellationToken ct = default) => throw new NotSupportedException();
    }
    #endregion
}
