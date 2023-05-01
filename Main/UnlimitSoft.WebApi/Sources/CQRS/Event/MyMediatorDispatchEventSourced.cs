using Microsoft.Extensions.DependencyInjection;
using System;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


/// <inheritdoc />
public sealed class MyMediatorDispatchEvent : MediatorDispatchEvent<EventPayload>
{
    private readonly IJsonSerializer _serializer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="serializer"></param>
    public MyMediatorDispatchEvent(IServiceProvider provider, IJsonSerializer serializer)
        : base(provider, false)
    {
        _serializer = serializer;
    }

    /// <inheritdoc />
    protected override EventPayload Create(IEvent @event) => new(@event, _serializer);
    /// <inheritdoc />
    protected override IEventRepository<EventPayload>? EventRepository => Provider.GetService<IMyEventSourcedRepository>();
}
