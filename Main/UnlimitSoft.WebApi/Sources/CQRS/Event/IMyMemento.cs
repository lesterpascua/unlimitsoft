using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public class MyMemento<TEntity> : Memento<TEntity, TEntity, JsonVersionedEventPayload, string>
    where TEntity : class, IEventSourced, new()
{
    public MyMemento(IJsonSerializer serializer, IEventNameResolver nameResolver, IEventSourcedRepository<JsonVersionedEventPayload, string> eventSourcedRepository, bool snapshot = false) : 
        base(serializer, nameResolver, eventSourcedRepository, null, snapshot)
    {
    }
}
