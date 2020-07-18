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
    public class DefaultMediatorDispatchEventSourced : IMediatorDispatchEventSourced
    {
        private readonly IServiceProvider _provider;
        private readonly IEventDispatcherWithServiceProvider _serviceProviderEventDispatcher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public DefaultMediatorDispatchEventSourced(IServiceProvider provider, IEventDispatcherWithServiceProvider serviceProviderEventDispatcher)
        {
            this._provider = provider;
            this._serviceProviderEventDispatcher = serviceProviderEventDispatcher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        public async Task DispatchEventsAsync(IEnumerable<IVersionedEvent> events)
        {
            foreach (var @event in events)
            {
                var responses = await this._serviceProviderEventDispatcher
                    .DispatchEventAsync(this._provider, @event);

                if (!responses.Success)
                {
                    var exceps = responses.ErrorEvents
                        .Select(s => (Exception)s.GetBody());
                    throw new AggregateException("Error when executed events", exceps);
                }
            }
        }
    }
}
