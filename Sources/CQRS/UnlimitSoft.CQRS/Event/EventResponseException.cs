using UnlimitSoft.CQRS.Message;
using System;

namespace UnlimitSoft.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public class EventResponseException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="response"></param>
        public EventResponseException(string message, IEventResponse response) : base(message)
        {
            Response = response;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEventResponse Response { get; }
    }
}
