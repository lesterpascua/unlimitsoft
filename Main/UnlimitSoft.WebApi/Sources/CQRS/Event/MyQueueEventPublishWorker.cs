using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


/// <inheritdoc />
public class MyQueueEventPublishWorker : QueueEventPublishWorker<IMyEventSourcedRepository, EventPayload>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="clock"></param>
    /// <param name="factory"></param>
    /// <param name="eventBus"></param>
    /// <param name="startDelay"></param>
    /// <param name="errorDelay"></param>
    /// <param name="logger"></param>
    /// <param name="bachSize"></param>
    public MyQueueEventPublishWorker(ISysClock clock, IServiceScopeFactory factory, IEventBus eventBus, TimeSpan? startDelay = null, TimeSpan? errorDelay = null, int bachSize = 10, ILogger<MyQueueEventPublishWorker>? logger = null)
        : base(clock, factory, eventBus, null, startDelay, errorDelay, bachSize, true, logger: logger)
    {
    }
}
