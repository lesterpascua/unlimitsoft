using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using System;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    /// <inheritdoc />
    public class MyMediatorDispatchEventSourced : JsonMediatorDispatchEventSourced
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public MyMediatorDispatchEventSourced(IServiceProvider provider)
            : base(provider, false)
        {
        }

        /// <inheritdoc />
        protected override IEventDispatcher EventDispatcher => Provider.GetService<IEventDispatcher>();
        /// <inheritdoc />
        protected override IEventPublishWorker EventPublishWorker => Provider.GetService<IEventPublishWorker>();
        /// <inheritdoc />
        protected override IEventSourcedRepository<JsonVersionedEventPayload, string> EventSourcedRepository => Provider.GetService<IMyEventSourcedRepository>();
    }
}
