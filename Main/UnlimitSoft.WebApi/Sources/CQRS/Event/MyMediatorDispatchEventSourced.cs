using Microsoft.Extensions.DependencyInjection;
using System;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


/// <inheritdoc />
public sealed class MyMediatorDispatchEventSourced : JsonMediatorDispatchEvent
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="serializer"></param>
    public MyMediatorDispatchEventSourced(IServiceProvider provider, IJsonSerializer serializer)
        : base(provider, serializer, false)
    {
    }

    /// <inheritdoc />
    protected override IEventDispatcher? EventDispatcher => Provider.GetService<IEventDispatcher>();
    /// <inheritdoc />
    protected override IEventPublishWorker? EventPublishWorker => Provider.GetService<IEventPublishWorker>();
    /// <inheritdoc />
    protected override IEventRepository<JsonEventPayload, string>? EventRepository => Provider.GetService<IMyEventSourcedRepository>();
}
