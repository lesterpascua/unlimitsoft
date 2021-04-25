using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
using System;
using System.Collections.Generic;
using System.Text;
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
        void Start(TimeSpan waitRetry);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task PublishEventAsync(IEvent @event);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="type">Messaje format type.</param>
        /// <returns></returns>
        Task PublishEventPayloadAsync<T>(EventPayload<T> @event, MessageType type);
        /// <summary>
        /// Publish a raw object in the bus
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="id"></param>
        /// <param name="eventName"></param>
        /// <param name="correlationId"></param>
        /// <returns></returns>
        Task PublishAsync(object graph, Guid id, string eventName, string correlationId);
    }
}
