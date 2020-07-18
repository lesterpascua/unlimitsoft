﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Message
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class CombinedEventResponse
    {
        /// <summary>
        /// Empty value
        /// </summary>
        public static readonly CombinedEventResponse Empty = new CombinedEventResponse(new EventResponse[0]);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        public CombinedEventResponse(IEnumerable<EventResponse> events)
        {
            this.Success = true;
            this.Events = events;

            List<EventResponse> errorEvents = new List<EventResponse>();
            List<EventResponse> successEvents = new List<EventResponse>();
            foreach (var @event in events)
            {
                if (!@event.Success)
                {
                    this.Success = false;
                    errorEvents.Add(@event);
                } else
                    successEvents.Add(@event);
            }

            this.Events = events;
            this.ErrorEvents = errorEvents;
            this.SuccessEvents = successEvents;
        }

        /// <summary>
        /// Indicate if all event are success.
        /// </summary>
        public bool Success { get; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<EventResponse> Events { get; }
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyCollection<EventResponse> ErrorEvents { get; }
        /// <summary>
        /// 
        /// </summary>
        public IReadOnlyCollection<EventResponse> SuccessEvents { get; }
    }
}