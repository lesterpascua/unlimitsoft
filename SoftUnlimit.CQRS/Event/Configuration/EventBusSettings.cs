using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class EventBusSettings
    {
        /// <summary>
        /// Indicate if publish in even bus is enable.
        /// </summary>
        public bool EventBusEnable { get; set; }
        /// <summary>
        /// Indicate if listen from event bus is enable.
        /// </summary>
        public bool EventListenerEnable { get; set; }

        /// <summary>
        /// If this value is true publish all event in local queue to check for processing. Normally you can 
        /// optimize the system handling local event directly and not throws queue.
        /// </summary>
        public bool AlsoPublishInOwnQueue { get; set; }
        /// <summary>
        /// Credential to autenticate in queue
        /// </summary>
        public Credential Credential { get; set; }
        /// <summary>
        /// Endpoint or connection
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// Owner queue name.
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// Queues for the system.
        /// </summary>
        public string[] EventQueues { get; set; }
    }
}
