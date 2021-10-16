using Newtonsoft.Json.Linq;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Helper methods for events.
    /// </summary>
    public static class IEventExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <returns></returns>
        public static T GetBody<T>(this IEvent @event) => JsonUtility.Cast<T>(@event.Body);

        /// <summary>
        /// Generate a success response using event data.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="skipEventInfo"></param>
        /// <returns></returns>
        public static IEventResponse OkResponse(this IEvent @event, bool skipEventInfo = true) => new EventResponse<object>(skipEventInfo ? null : @event, 200, null);

        /// <summary>
        /// Generate a success response using event data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="body"></param>
        /// <param name="skipEventInfo"></param>
        /// <returns></returns>
        public static IEventResponse OkResponse<T>(this IEvent @event, T body, bool skipEventInfo = true) => new EventResponse<T>(skipEventInfo ? null : @event, 200, body);
        /// <summary>
        /// Generate a error response using event data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="event"></param>
        /// <param name="body"></param>
        /// <param name="skipCommandInfo"></param>
        /// <returns></returns>
        public static IEventResponse ErrorResponse<T>(this IEvent @event, T body, bool skipCommandInfo = true) => new EventResponse<T>(skipCommandInfo ? null : @event, 500, body);
    }
}
