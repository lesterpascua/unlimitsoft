using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using System;

namespace App.Manual.Tests.CQRS.Events
{
    public class DefaultMediatorDispatchEventSourced : JsonMediatorDispatchEventSourced
    {
        public DefaultMediatorDispatchEventSourced(IServiceProvider provider)
            : base(provider, typeof(IUnitOfWork))
        {
        }

        protected override IEventDispatcher EventDispatcher => Provider.GetService(typeof(IEventDispatcher)) as IEventDispatcher;

        protected override IRepository<JsonVersionedEventPayload> VersionedEventRepository => Provider.GetService(typeof(IRepository<JsonVersionedEventPayload>)) as IRepository<JsonVersionedEventPayload>;

        protected override IEventPublishWorker EventPublishWorker => null;
    }
}
