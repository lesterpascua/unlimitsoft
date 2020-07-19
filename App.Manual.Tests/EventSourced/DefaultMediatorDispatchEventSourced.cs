using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Test.Data;
using SoftUnlimit.CQRS.Test.Model;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;
using SoftUnlimit.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Test.EventSourced
{
    public class MyDefaultMediatorDispatchEventSourced : DefaultMediatorDispatchEventSourced
    {
        private readonly IServiceProvider _provider;
        private readonly IEventDispatcherWithServiceProvider _serviceProviderEventDispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public MyDefaultMediatorDispatchEventSourced(IServiceProvider provider, IEventDispatcherWithServiceProvider serviceProviderEventDispatcher)
            : base(provider)
        {
            this._provider = provider;
            this._serviceProviderEventDispatcher = serviceProviderEventDispatcher;
        }

        protected override IEventDispatcherWithServiceProvider EventDispatcher => throw new NotImplementedException();

        protected override IRepository<VersionedEventPayload> VersionedEventRepository => throw new NotImplementedException();
    }
}
