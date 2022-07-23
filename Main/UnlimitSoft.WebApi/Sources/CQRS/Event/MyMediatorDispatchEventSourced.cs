using Microsoft.Extensions.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.Data;
using System;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event
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
