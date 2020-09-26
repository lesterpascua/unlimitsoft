using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public static class IEventExtensions
    {
        /// <summary>
        /// Generate a success response using event data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="body"></param>
        /// <param name="skipEventInfo"></param>
        /// <returns></returns>
        public static EventResponse OkResponse<T>(this IEvent @event, T body, bool skipEventInfo = true) => new EventResponse<T>(skipEventInfo ? null : @event, 200, body);
        /// <summary>
        /// Generate a error response using event data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="body"></param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static EventResponse ErrorResponse<T>(this IEvent @event, T body, bool skipCommandInfo = true) => new EventResponse<T>(skipCommandInfo ? null : @event, 500, body);
    }
}
