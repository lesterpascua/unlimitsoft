using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Memento.Json;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


/// <summary>
/// memento
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public sealed class MyMemento<TEntity> : JsonMemento<TEntity, TEntity, EventPayload>
    where TEntity : class, IEventSourced, new()
{
    public MyMemento(IJsonSerializer serializer, IEventNameResolver nameResolver, IEventRepository<EventPayload> eventSourcedRepository) : 
        base(serializer, nameResolver, eventSourcedRepository, null)
    {
    }
}
