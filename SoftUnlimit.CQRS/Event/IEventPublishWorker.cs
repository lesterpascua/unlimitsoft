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
        void Init();
        /// <summary>
        /// Initialize worker
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken ct = default);

        /// <summary>
        /// Add collection of events to worker.
        /// </summary>
        /// <param name="events"></param>
        void Publish(IEnumerable<IEvent> events);
    }
}
