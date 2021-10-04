using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using System;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public class MyQueueEventPublishWorker<TUnitOfWork> : QueueEventPublishWorker<TUnitOfWork, IMyVersionedEventRepository, JsonVersionedEventPayload, string>
        where TUnitOfWork : IUnitOfWork
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
        public MyQueueEventPublishWorker(IServiceScopeFactory factory, IEventBus eventBus, MessageType type, TimeSpan? checkTime = null, TimeSpan? errorDelay = null, int bachSize = 10, ILogger<MyQueueEventPublishWorker<TUnitOfWork>> logger = null)
            : base(factory, eventBus, type, checkTime, errorDelay, bachSize, logger)
        {
        }
    }
}
