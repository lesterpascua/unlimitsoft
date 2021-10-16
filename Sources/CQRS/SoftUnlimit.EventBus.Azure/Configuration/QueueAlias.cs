using System;

namespace SoftUnlimit.EventBus.Azure.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class QueueAlias<TAlias>
        where TAlias : Enum
    {
        /// <summary>
        /// Indicate if the queue is active in this service.
        /// </summary>
        public bool? Active { get; set; }
        /// <summary>
        /// Real name of the queue. By default match with alias
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// Alias asociate to the queue.
        /// </summary>
        public TAlias Alias { get; set; }
    }
}
