using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.CQRS.Memento;
using SoftUnlimit.Event;
using SoftUnlimit.Json;
using System;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public class MyMemento<TEntity> : Memento<TEntity, TEntity, JsonVersionedEventPayload, string>
        where TEntity : class, IEventSourced, new()
    {
        public MyMemento(IEventNameResolver nameResolver, IEventSourcedRepository<JsonVersionedEventPayload, string> eventSourcedRepository, bool snapshot = false) : 
            base(nameResolver, eventSourcedRepository, snapshot)
        {
        }

        protected override IMementoEvent<TEntity> FromEvent(Type type, string payload)
        {
            return (IMementoEvent<TEntity>)JsonUtility.Deserialize(type, payload);
        }
    }
}
