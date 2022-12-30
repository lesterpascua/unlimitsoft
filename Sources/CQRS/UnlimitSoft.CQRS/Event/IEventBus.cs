using UnlimitSoft.Event;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// When implement start event bus connection. If connection fail should recover.
    /// </summary>
    /// <param name="waitRetry">If fail indicate time to wait until retry again.</param>
    /// <param name="ct"></param>
    ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct = default);

    /// <summary>
    /// Publis event in bus.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="useEnvelop"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task PublishAsync(IEvent @event, bool useEnvelop = true, CancellationToken ct = default);
    /// <summary>
    /// Publis event in bus.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="type">Messaje format type.</param>
    /// <param name="useEnvelop"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task PublishPayloadAsync<T>(EventPayload<T> @event, MessageType type, bool useEnvelop = true, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="id"></param>
    /// <param name="eventName"></param>
    /// <param name="correlationId"></param>
    /// <param name="useEnvelop"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task PublishAsync(object graph, Guid id, string eventName, string correlationId, bool useEnvelop = true, CancellationToken ct = default);
}
