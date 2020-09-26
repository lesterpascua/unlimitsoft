using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event.Configuration
{
    /// <summary>
    /// Type and setting of bus to be use.
    /// </summary>
    public class EventBusTypeSettings
    {
        /// <summary>
        /// Event bust type
        /// </summary>
        public EventBusType Type { get; set; }
        /// <summary>
        /// Event bus access settings.
        /// </summary>
        public EventBusSettings Settings { get; set; }
    }
}
