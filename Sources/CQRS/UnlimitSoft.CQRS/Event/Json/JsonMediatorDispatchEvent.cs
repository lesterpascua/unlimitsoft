using System;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event.Json;


/// <summary>
/// 
/// </summary>
public abstract class JsonMediatorDispatchEvent : MediatorDispatchEvent<JsonEventPayload, string>
{
    private readonly IJsonSerializer _serializer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="serializer"></param>
    /// <param name="directlyDispatchNotDomainEvents">If true not dispath domain event as optimized mechanims.</param>
    public JsonMediatorDispatchEvent(IServiceProvider provider, IJsonSerializer serializer, bool directlyDispatchNotDomainEvents = false)
        : base(provider, directlyDispatchNotDomainEvents)
    {
        _serializer = serializer;
    }

    /// <summary>
    /// Create new event using versioned event as template.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    protected override JsonEventPayload Create(IEvent @event) => new(@event, _serializer);
}
