using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.CQRS.EventSourcing.Json;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using System;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event
{
    public class MyMemento<TEntity> : Memento<TEntity, TEntity, JsonVersionedEventPayload, string>
        where TEntity : class, IEventSourced, new()
    {
        public MyMemento(IEventNameResolver nameResolver, IEventSourcedRepository<JsonVersionedEventPayload, string> eventSourcedRepository, bool snapshot = false) : 
            base(nameResolver, eventSourcedRepository, null, snapshot)
        {
        }

        protected override IMementoEvent<TEntity> FromEvent(Type type, string payload)
        {
            return (IMementoEvent<TEntity>)JsonUtility.Deserialize(type, payload);
        }
    }
}
