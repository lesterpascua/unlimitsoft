﻿using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task PublishEventAsync(IEvent @event);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task PublishJsonEventPayloadAsync(JsonEventPayload @event);
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task PublishBinaryEventPayloadAsync(BinaryEventPayload @event);
    }
}
