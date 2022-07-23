﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.EventSourcing.Json;
using System;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


/// <inheritdoc />
public class MyQueueEventPublishWorker : QueueEventPublishWorker<IMyEventSourcedRepository, JsonVersionedEventPayload, string>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="eventBus"></param>
    /// <param name="type"></param>
    /// <param name="checkTime"></param>
    /// <param name="errorDelay"></param>
    /// <param name="logger"></param>
    /// <param name="bachSize"></param>
    public MyQueueEventPublishWorker(IServiceScopeFactory factory, IEventBus eventBus, MessageType type, TimeSpan? checkTime = null, TimeSpan? errorDelay = null, int bachSize = 10, ILogger<MyQueueEventPublishWorker> logger = null)
        : base(factory, eventBus, type, null, checkTime, errorDelay, bachSize, true, true, logger)
    {
    }
}
