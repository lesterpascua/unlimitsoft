using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// When implement start event bus connection. If connection fail should recover.
        /// </summary>
        /// <param name="waitRetry">If fail indicate time to wait until retry again.</param>
        [Obsolete("Use StartAsync")]
        void Start(TimeSpan waitRetry);

        /// <summary>
        /// When implement start event bus connection. If connection fail should recover.
        /// </summary>
        /// <param name="waitRetry">If fail indicate time to wait until retry again.</param>
        /// <param name="ct"></param>
        Task StartAsync(TimeSpan waitRetry, CancellationToken ct = default);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishEventAsync(IEvent @event, CancellationToken ct = default);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="type">Messaje format type.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishEventPayloadAsync<T>(EventPayload<T> @event, MessageType type, CancellationToken ct = default);
        /// <summary>
        /// Publish a raw object in the bus
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="id"></param>
        /// <param name="eventName"></param>
        /// <param name="correlationId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishAsync(object graph, Guid id, string eventName, string correlationId, CancellationToken ct = default);
    }
}
