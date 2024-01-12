using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public sealed class PublishEventInfo : IComparable<PublishEventInfo>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="created"></param>
    /// <param name="scheduled"></param>
    public PublishEventInfo(Guid id, DateTime created, DateTime? scheduled)
    {
        Id = id;
        Created = created;
        Scheduled = scheduled;
    }

    /// <summary>
    /// Identifier of the event
    /// </summary>
    public Guid Id { get; }
    /// <summary>
    /// Date where the event is scheduled
    /// </summary>
    public DateTime? Scheduled { get; init; }
    /// <summary>
    /// Date where the event is created
    /// </summary>
    public DateTime Created { get; init; }

    /// <inheritdoc />
    public int CompareTo(PublishEventInfo? other)
    {
        if (other is null)
            throw new ArgumentNullException(nameof(other), "Compare operand can't be null");

        var date1 = Scheduled ?? Created;
        var date2 = other.Scheduled ?? other.Created;

        var value = date1.CompareTo(date2);
        if (value == 0)
            return Id.CompareTo(other.Id);
        return value;
    }
    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not PublishEventInfo k)
            return false;
        return Id == k.Id;
    }
}

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
    Task RetryPublishAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Add collection of events to worker. To publish in the bus.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task PublishAsync(IEnumerable<IEvent> events, CancellationToken ct = default);
    /// <summary>
    /// Add collection of events to worker. To publish in the bus.
    /// </summary>
    /// <param name="events"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task PublishAsync(IEnumerable<PublishEventInfo> events, CancellationToken ct = default);
}
