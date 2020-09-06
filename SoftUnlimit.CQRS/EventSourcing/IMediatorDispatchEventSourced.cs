﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// Used as proxy to store versioned event generated by entities just before complete transaction
    /// </summary>
    public interface IMediatorDispatchEventSourced
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events);
        /// <summary>
        /// When all event are saved invoqued this method.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task EventsDispatchedAsync(IEnumerable<IVersionedEvent> events);
    }
}
