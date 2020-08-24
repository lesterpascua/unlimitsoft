using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimited.EventBus.ActiveMQ
{
    [Serializable]
    public class MessageEvelop
    {
        /// <summary>
        /// 
        /// </summary>
        public MessageType Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public object Messaje { get; set; }
    }
}
