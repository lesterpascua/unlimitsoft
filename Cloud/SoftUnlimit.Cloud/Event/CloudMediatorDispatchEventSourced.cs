using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using System;

namespace SoftUnlimit.Cloud.Event
{
    /// <inheritdoc />
    public class CloudMediatorDispatchEventSourced<TUnitOfWork> : JsonMediatorDispatchEventSourced
        where TUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="eventBusTypeOption"></param>
        public CloudMediatorDispatchEventSourced(IServiceProvider provider)
            : base(provider, typeof(TUnitOfWork), false)
        {
        }

        /// <inheritdoc />
        protected override IUnitOfWork UnitOfWork => Provider.GetService<TUnitOfWork>();
        /// <inheritdoc />
        protected override IEventDispatcher EventDispatcher => Provider.GetService<IEventDispatcher>();
        /// <inheritdoc />
        protected override IEventPublishWorker EventPublishWorker => Provider.GetService<IEventPublishWorker>();
        /// <inheritdoc />
        protected override IRepository<JsonVersionedEventPayload> VersionedEventRepository => Provider.GetService<ICloudVersionedEventRepository>();
    }
}
