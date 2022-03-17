using System;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Messaje envelop with messaje metadata.
    /// </summary>
    public class MessageEnvelop
    {
        private MessageType _type;
        private object _msg;
        private string _msgType;

        /// <summary>
        /// 
        /// </summary>
        public MessageType Type { get => _type; set => _type = value; }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use MessageType")]
        public object Messaje { get => _msg; set => _msg = value; }
        /// <summary>
        /// 
        /// </summary>
        public object Msg { get => _msg; set => _msg = value; }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("Use MessageType")]
        public string MessajeType { get => _msgType; set => _msgType = value; }
        /// <summary>
        /// 
        /// </summary>
        public string MsgType { get => _msgType; set => _msgType = value; }
    }
}
