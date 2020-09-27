using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Messaje envelopt
    /// </summary>
    [Serializable]
    public class MessageEnvelop
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageType Type { get; set; }

        /// <summary>
        /// Time where message was created.
        /// </summary>
        public DateTime? Created { get; set; }
        /// <summary>
        /// Service were message is created.
        /// </summary>
        public uint? ServiceId { get; set; }
        /// <summary>
        /// Worker where messaje is created.
        /// </summary>
        public string WorkerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object Messaje { get; set; }
    }
}
