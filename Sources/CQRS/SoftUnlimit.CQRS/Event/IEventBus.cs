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
        /// <param name="ct"></param>
        ValueTask StartAsync(TimeSpan waitRetry, CancellationToken ct = default);

        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishAsync(IEvent @event, CancellationToken ct = default);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="type">Messaje format type.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task PublishPayloadAsync<T>(EventPayload<T> @event, MessageType type, CancellationToken ct = default);
    }
}
