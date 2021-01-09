using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Messaje envelop with messaje metadata.
    /// </summary>
    [Serializable]
    public class MessageEnvelop
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageType Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object Messaje { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MessajeType { get; set; }
    }
}
