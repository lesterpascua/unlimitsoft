using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Identifier for entity owner of this event. Avoid diferent entitities with equal SourceID generate collitions.
        /// </summary>
        long EntityID { get; }
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        object SourceID { get; }
        /// <summary>
        /// Specify if an event belown to domain. This have optimization propouse.
        /// </summary>
        bool IsDomainEvent { get; }
        /// <summary>
        /// Event information
        /// </summary>
        public object Body { get; }
    }

    /// <summary>
    /// Represents an event message.
    /// </summary>
    public abstract class Event<Key> : IEvent
    {
        /// <summary>
        /// 
        /// </summary>
        protected Event()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="sourceID"></param>
        /// <param name="isDomain"></param>
        /// <param name="body"></param>
        protected Event(long entityID, Key sourceID, bool isDomain, object body)
        {
            this.EntityID = entityID;
            this.SourceID = sourceID;
            this.IsDomainEvent = isDomain;
            this.Body = body;
        }

        /// <summary>
        /// Identifier for entity owner of this event. Avoid diferent entitities with equal SourceID generate collitions.
        /// </summary>
        public long EntityID { get; protected set; }
        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public Key SourceID { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsDomainEvent { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public object Body { get; protected set; }


        #region Explicit Interface Implementation

        object IEvent.SourceID => this.SourceID;

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IEventExtenssion
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
