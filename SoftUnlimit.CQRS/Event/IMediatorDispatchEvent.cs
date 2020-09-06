﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Used as proxy to store event generated by entities just before complete transaction
    /// </summary>
    public interface IMediatorDispatchEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task DispatchEventsAsync(IEnumerable<IEvent> entities);
        /// <summary>
        /// When all event are saved invoqued this method. 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task EventsDispatchedAsync(IEnumerable<IEvent> entities);
    }
}
