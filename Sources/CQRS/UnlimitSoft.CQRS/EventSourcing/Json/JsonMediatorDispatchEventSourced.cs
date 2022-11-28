using UnlimitSoft.Event;
using System;
using UnlimitSoft.Json;

namespace UnlimitSoft.CQRS.EventSourcing.Json;


/// <summary>
/// 
/// </summary>
public abstract class JsonMediatorDispatchEventSourced : MediatorDispatchEventSourced<JsonVersionedEventPayload, string>
{
    private readonly IJsonSerializer _serializer;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="serializer"></param>
    /// <param name="directlyDispatchNotDomainEvents">If true not dispath domain event as optimized mechanims.</param>
    public JsonMediatorDispatchEventSourced(IServiceProvider provider, IJsonSerializer serializer, bool directlyDispatchNotDomainEvents = false)
        : base(provider, directlyDispatchNotDomainEvents)
    {
        _serializer = serializer;
    }

    /// <summary>
    /// Create new event using versioned event as template.
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    protected override JsonVersionedEventPayload Create(IVersionedEvent @event) => new(@event, _serializer);
}
