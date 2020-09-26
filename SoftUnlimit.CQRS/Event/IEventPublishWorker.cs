using System;
using System.Collections.Generic;
using System.Text;

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
        public void Init();
        /// <summary>
        /// Add collection of events to worker.
        /// </summary>
        /// <param name="events"></param>
        void Publish(IEnumerable<IEvent> events);
    }
}
