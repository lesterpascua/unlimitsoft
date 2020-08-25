using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimited.EventBus.ActiveMQ.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class EventBusSettings
    {
        /// <summary>
        /// Indicate if enable event bus.
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// Also register listener.
        /// </summary>
        public bool EnableListener { get; set; }
        /// <summary>
        /// Indicate message will publish in all queue except service queue. Some optimization allow handle event from currect service directly avoinding overload
        /// of publich in queue for later processing.
        /// </summary>
        public bool ExcludeFromPublishMyQueue { get; set; }
        /// <summary>
        /// ActiveMQ user
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// ActiveMQ password.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Endpoint used for activeMQ
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// Queue user for listener messajes
        /// </summary>
        public string Queue { get; set; }
        /// <summary>
        /// Queue where messaje are send.
        /// </summary>
        public string[] EventQueues { get; set; }
    }
}
