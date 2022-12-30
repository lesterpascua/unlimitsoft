using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public class MyMemento<TEntity> : Memento<TEntity, TEntity, JsonEventPayload, string>
    where TEntity : class, IEventSourced, new()
{
    public MyMemento(IJsonSerializer serializer, IEventNameResolver nameResolver, IEventSourcedRepository<JsonEventPayload, string> eventSourcedRepository, bool snapshot = false) : 
        base(serializer, nameResolver, eventSourcedRepository, null, snapshot)
    {
    }
}
