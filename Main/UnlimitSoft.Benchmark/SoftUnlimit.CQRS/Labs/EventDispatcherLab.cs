﻿using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Validation;
using SoftUnlimit.Event;
using SoftUnlimit.Web.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Benchmark.SoftUnlimit.CQRS.Labs
{
    /// <summary>
    /// Test UnlimitSoftDispatcher vs MediatR
    /// </summary>
    public class EventDispatcherLab
    {
        private readonly IServiceProvider _provider;
        private readonly IEventDispatcher _dispatcher;

        public EventDispatcherLab()
        {
            var services = new ServiceCollection();

            services.AddEventHandler(typeof(IEventHandler<>), null, typeof(Program).Assembly);

            _provider = services.BuildServiceProvider();
            _dispatcher = _provider.GetRequiredService<IEventDispatcher>();
        }

        public async Task<string> Dispatch()
        {
            var @event = new Event { Name = "Lester Pastrana" };
            var response = await _dispatcher.DispatchAsync(@event);

            return response.GetBody<string>();
        }

        #region Nested Classes
        /// <summary>
        /// 
        /// </summary>
        public class Event : VersionedEvent<Guid, string>
        {
        }
        public class EventHandler : IEventHandler<Event>
        {
            public Task<IEventResponse> HandleAsync(Event @event, CancellationToken ct = default)
            {
                var result = $"{@event.Body} - {DateTime.UtcNow}";
                return Task.FromResult(@event.OkResponse(result));
            }
        }
        #endregion
    }
}