using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.Memento.Json;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


/// <summary>
/// memento
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public sealed class MyMemento<TEntity> : JsonMemento<TEntity, TEntity>
    where TEntity : class, IEventSourced, new()
{
    public MyMemento(IJsonSerializer serializer, IEventNameResolver nameResolver, IEventRepository<JsonEventPayload, string> eventSourcedRepository) : 
        base(serializer, nameResolver, eventSourcedRepository, null)
    {
    }
}
