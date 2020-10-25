using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
