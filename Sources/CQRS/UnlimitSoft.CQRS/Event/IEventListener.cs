﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public interface IEventListener
{
    /// <summary>
    /// Enter a background process for listener loop of event. If connection fail should recover.
    /// </summary>
    /// <param name="waitRetry">If fail indicate time to wait until retry again. This value has to be lower then the max allowed for the implemented listener.</param>
    /// <param name="ct"></param>
    ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default);
}
