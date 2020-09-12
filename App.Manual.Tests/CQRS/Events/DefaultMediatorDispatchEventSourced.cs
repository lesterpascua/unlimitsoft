using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Binary;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App.Manual.Tests.CQRS.Events
{
    public class DefaultMediatorDispatchEventSourced : BinaryMediatorDispatchEventSourced
    {
        public DefaultMediatorDispatchEventSourced(IServiceProvider provider)
            : base(provider)
        {
        }

        protected override IEventDispatcherWithServiceProvider EventDispatcher => Provider.GetService(typeof(IEventDispatcherWithServiceProvider)) as IEventDispatcherWithServiceProvider;
        protected override IRepository<BinaryVersionedEventPayload> VersionedEventRepository => Provider.GetService(typeof(IRepository<BinaryVersionedEventPayload>)) as IRepository<BinaryVersionedEventPayload>;
    }
}
