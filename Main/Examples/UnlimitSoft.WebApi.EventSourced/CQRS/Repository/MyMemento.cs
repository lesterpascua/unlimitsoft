using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Memento.Json;
using UnlimitSoft.Event;
using UnlimitSoft.Json;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Repository;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TInterface"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public sealed class MyMemento<TInterface, TEntity> : JsonMemento<TInterface, TEntity>
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
}