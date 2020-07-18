using SoftUnlimit.CQRS.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// Base class for all EventResponse 
    /// </summary>
    public abstract class EventResponse
    {
        /// <summary>
        /// 
        /// </summary>
        internal protected EventResponse() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="code"></param>
        internal protected EventResponse(IEvent @event, int code)
        {
            this.Code = code;
            this.Event = @event;
        }

        /// <summary>
        /// Http response code for execution of command.
        /// </summary>
        public int Code { get; protected set; }
        /// <summary>
        /// Command where response is created.
        /// </summary>
        public IEvent Event { get; protected set; }
        /// <summary>
        /// Indicate if event is success. This is only when code beteen 200 and 299.
        /// </summary>
        public bool Success => 200 <= this.Code && this.Code <= 299;

        /// <summary>
        /// Get body.
        /// </summary>
        /// <returns></returns>
        public abstract object GetBody();
        /// <summary>
        /// Get type of body
        /// </summary>
        /// <returns></returns>
        public abstract Type GetBodyType();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventResponse<T> : EventResponse
    {
        /// <summary>
        /// 
        /// </summary>
        public EventResponse() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        public EventResponse(IEvent @event, int code, T body)
            : base(@event, code)
        {
            this.Body = body;
        }

        /// <summary>
        /// 
        /// </summary>
        public T Body { get; protected set; }

        /// <summary>
        /// Get body.
        /// </summary>
        /// <returns></returns>
        public override object GetBody() => this.Body;
        /// <summary>
        /// Get type of body
        /// </summary>
        /// <returns></returns>
        public override Type GetBodyType() => this.Body?.GetType();
    }
}
