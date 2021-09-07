using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Create a backgound process to publish all dispatcher events.
    /// </summary>
    public interface IEventPublishWorker
    {
        /// <summary>
        /// Initialize worker.
        /// </summary>
        [Obsolete("Use async version")]
        void Init();
        /// <summary>
        /// Add collection of events to worker.
        /// </summary>
        /// <param name="events"></param>
        [Obsolete("Use async version")]
        void Publish(IEnumerable<IEvent> events);


        /// <summary>
        /// Initialize worker
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken ct = default);
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
    }
}
