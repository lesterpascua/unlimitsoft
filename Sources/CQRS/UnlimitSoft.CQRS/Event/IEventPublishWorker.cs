using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="Created"></param>
/// <param name="Scheduled"></param>
public sealed record PublishEventInfo(Guid Id, DateTime Created, DateTime? Scheduled = null);

/// <summary>
/// Create a backgound process to publish all dispatcher events.
/// </summary>
public interface IEventPublishWorker : IDisposable
{
    /// <summary>
    /// Amount of event pending to sent
    /// </summary>
    int Pending { get; }

    /// <summary>
    /// Initialize worker
    /// </summary>
    /// <param name="loadEvent">
    /// If service has multiples instance and there is pending event when start will be a problem because the event will load multiples times. Only
    /// set true to one service to avoid send duplicate event.
    /// </param>
    /// <param name="bachSize">Load event identifier using this bach size.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task StartAsync(bool loadEvent, int bachSize = 1000, CancellationToken ct = default);
    /// <summary>
    /// Retry send some event already publish.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask RetryPublishAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Add collection of events to worker. To publish in the bus.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask PublishAsync(IEnumerable<IEvent> events, CancellationToken ct = default);
    /// <summary>
    /// Add collection of events to worker. To publish in the bus.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask PublishAsync(IEnumerable<PublishEventInfo> events, CancellationToken ct = default);
}
