using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.Event;
using UnlimitSoft.Json;

namespace UnlimitSoft.CQRS.Memento.Json;



/// <summary>
/// Load the entity in some moment of the system history.
/// </summary>
/// <typeparam name="TInterface"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public abstract class JsonMemento<TInterface, TEntity> : IMemento<TInterface>
    where TEntity : class, TInterface, IEventSourced, new()
{
    private readonly IJsonSerializer _serializer;
    private readonly IEventNameResolver _nameResolver;
    private readonly IEventRepository<JsonEventPayload, string> _eventSourcedRepository;
    private readonly Func<IReadOnlyCollection<IMementoEvent<TInterface>>, TEntity>? _factory;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer">Json serializer used to convert the event</param>
    /// <param name="nameResolver"></param>
    /// <param name="eventSourcedRepository"></param>
    /// <param name="factory">Allow personalice the wait you use to build the entity. By the fault the entity will build using the default ctor.</param>
    public JsonMemento(
        IJsonSerializer serializer,
        IEventNameResolver nameResolver,
        IEventRepository<JsonEventPayload, string> eventSourcedRepository,
        Func<IReadOnlyCollection<IMementoEvent<TInterface>>, TEntity>? factory = null
    )
    {
        _serializer = serializer;
        _nameResolver = nameResolver;
        _eventSourcedRepository = eventSourcedRepository;
        _factory = factory;
    }

    /// <inheritdoc />
    public async Task<TInterface?> FindByVersionAsync(Guid id, long? version = null, CancellationToken ct = default)
    {
        var eventsPayload = await _eventSourcedRepository.GetHistoryAsync(id, version ?? long.MaxValue, ct);
        if (eventsPayload?.Any() != true)
            return default;
        return LoadEntityFromHistory(eventsPayload);
    }
    /// <inheritdoc />
    public async Task<TInterface?> FindByCreateAsync(Guid id, DateTime? dateTime = null, CancellationToken ct = default)
    {
        var eventsPayload = await _eventSourcedRepository.GetHistoryAsync(id, dateTime ?? DateTime.MaxValue, ct);
        if (eventsPayload?.Any() != true)
            return default;
        return LoadEntityFromHistory(eventsPayload);
    }

    /// <summary>
    /// Get IMementoEvent from the event payload. 
    /// </summary>
    /// <remarks>
    /// By default the system use a json deserialization. If you use a diferent serialization you need to override this method.
    /// </remarks>
    /// <param name="type"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    protected virtual IMementoEvent<TInterface> FromEvent(Type type, string payload) => (IMementoEvent<TInterface>)_serializer.Deserialize(type, payload)!;


    #region Nested Classes
    private TEntity LoadEntityFromHistory(JsonEventPayload[] eventsPayload)
    {
        var history = new IMementoEvent<TInterface>[eventsPayload.Length];
        for (var i = 0; i < eventsPayload.Length; i++)
        {
            var eventPayload = eventsPayload[i];
            var eventType = _nameResolver.Resolver(eventPayload.EventName);
            if (eventType is null)
                throw new KeyNotFoundException($"Event name={eventPayload.EventName} can't resolve");

            history[i] = FromEvent(eventType, eventPayload.Body);
        }

        var entity = _factory?.Invoke(history) ?? new TEntity();
        for (var i = 0; i < history.Length; i++)
            history[i].Apply(entity);
        return entity;
    }
    #endregion
}