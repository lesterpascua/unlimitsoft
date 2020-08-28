﻿using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Indicate this is a final event in saga process.
    /// </summary>
    public interface IFinalEvent : IEvent
    {
        /// <summary>
        /// Create response for command that create event saga
        /// </summary>
        /// <returns></returns>
        CommandResponse GetResponse();
    }
}
