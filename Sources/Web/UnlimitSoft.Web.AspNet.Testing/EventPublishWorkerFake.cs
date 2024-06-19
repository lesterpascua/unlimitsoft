using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.AspNet.Testing;


/// <summary>
/// 
/// </summary>
public sealed class EventPublishWorkerFake : IEventPublishWorker
{
    private readonly IEventBus _eventBus;

    /// <inheritdoc />
    public EventPublishWorkerFake(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <inheritdoc />
    public int Pending => 0;


    /// <inheritdoc />
    public void Dispose() { }

    /// <inheritdoc />
    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken ct = default)
    {
        foreach (var @event in events)
            await _eventBus.PublishAsync(@event, ct: ct);
    }
    /// <inheritdoc />
    public Task PublishAsync(IEnumerable<PublishEventInfo> events, CancellationToken ct = default) => Task.CompletedTask;
    /// <inheritdoc />
    public Task RetryPublishAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;
    /// <inheritdoc />
    public Task StartAsync(bool loadEvent, int bachSize = 1000, CancellationToken ct = default) => Task.CompletedTask;
}
