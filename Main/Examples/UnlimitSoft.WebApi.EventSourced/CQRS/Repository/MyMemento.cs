using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Memento.Json;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.EventSourced.CQRS.Event;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Repository;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TInterface"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public sealed class MyMemento<TInterface, TEntity> : JsonMemento<TInterface, TEntity, MyEventPayload>
    where TEntity : class, TInterface, IEventSourced, new()
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="nameResolver"></param>
    /// <param name="eventSourcedRepository"></param>
    /// <param name="factory"></param>
    public MyMemento(IJsonSerializer serializer, IEventNameResolver nameResolver, IMyEventRepository eventSourcedRepository, Func<IReadOnlyCollection<IMementoEvent<TInterface>>, TEntity> factory)
        : base(serializer, nameResolver, eventSourcedRepository, factory)
    {
    }

    protected override IMementoEvent<TInterface> FromEvent(Type type, MyEventPayload payload)
    {
        var e = base.FromEvent(type, payload);
        ((IMyEvent)e).Text = payload.Text;

        return e;
    }
}