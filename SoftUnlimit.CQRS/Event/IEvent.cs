﻿using SoftUnlimit.CQRS.Command;
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
        /// Gets the identifier of the source originating the event.
        /// </summary>
        object SourceId { get; set; }
        /// <summary>
        /// Identifier of service where event is created
        /// </summary>
        uint ServiceId { get; }
        /// <summary>
        /// Identifier of the worker were the event is create.
        /// </summary>
        string WorkerId { get; }
        /// <summary>
        /// Event creation date
        /// </summary>
        DateTime Created { get; }
        /// <summary>
        /// Event name general is the type fullname
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Command where event is originate.
        /// </summary>
        ICommand Creator { get; }
        /// <summary>
        /// Previous snapshot in json representation.
        /// </summary>
        object PrevState { get; }
        /// <summary>
        /// Currenct snapshot in json representation
        /// </summary>
        object CurrState { get; }
        /// <summary>
        /// Specify if an event belown to domain. This have optimization propouse.
        /// </summary>
        bool IsDomainEvent { get; }
        /// <summary>
        /// Event extra information
        /// </summary>
        public object Body { get; }
    }

    /// <summary>
    /// Represents an event message.
    /// </summary>
    [Serializable]
    public abstract class Event<TKey> : IEvent
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
        /// <param name="sourceId"></param>
        /// <param name="serviceId"></param>
        /// <param name="workerId"></param>
        /// <param name="command"></param>
        /// <param name="prevState"></param>
        /// <param name="currState"></param>
        /// <param name="isDomain"></param>
        /// <param name="body"></param>
        protected Event(TKey sourceId, uint serviceId, string workerId, ICommand command, object prevState, object currState, bool isDomain, object body)
        {
            SourceId = sourceId;
            ServiceId = serviceId;
            WorkerId = workerId;

            Creator = command;
            PrevState = prevState;
            CurrState = currState;

            IsDomainEvent = isDomain;
            Body = body;
        }

        /// <summary>
        /// Gets the identifier of the source originating the event.
        /// </summary>
        public TKey SourceId { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public uint ServiceId { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public string WorkerId { get; protected set; }
        /// <summary>
        /// Event creation date
        /// </summary>
        public DateTime Created { get; protected set; }
        /// <summary>
        /// Get name of event
        /// </summary>
        public string Name => this.GetType().FullName;

        /// <summary>
        /// 
        /// </summary>
        public ICommand Creator { get; protected set; }
        /// <summary>
        /// Previous snapshot of entity.
        /// </summary>
        public object PrevState { get; protected set; }
        /// <summary>
        /// Currenct snapshot of entity.
        /// </summary>
        public object CurrState { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDomainEvent { get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        public object Body { get; protected set; }


        /// <summary>
        /// Convert objeto to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name;


        #region Explicit Interface Implementation

        object IEvent.SourceId { get => this.SourceId; set => this.SourceId = (TKey)value; }

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
