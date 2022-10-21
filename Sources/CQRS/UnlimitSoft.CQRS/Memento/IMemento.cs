using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.EventSourcing;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Memento;


/// <summary>
/// Alow to the build some entity using all event applied in the history. 
/// </summary>
/// <remarks>The event will be applied in the order of receive.</remarks>
/// <typeparam name="TEntity"></typeparam>
public interface IMemento<TEntity>
{
    /// <summary>
    /// Build entity in the moment of the version supplied
    /// </summary>
    /// <param name="id"></param>
    /// <param name="version">Version of the entity.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEntity?> FindByVersionAsync(string id, long? version = null, CancellationToken ct = default);
    /// <summary>
    /// Build entity in the moment of the date supplied.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dateTime">Date where we need to check the entity.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task<TEntity?> FindByCreateAsync(string id, DateTime? dateTime = null, CancellationToken ct = default);
}
/// <summary>
/// Load the entity in some moment of the system history.
/// </summary>
/// <typeparam name="TInterface"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TVersionedEventPayload"></typeparam>
/// <typeparam name="TPayload"></typeparam>
public abstract class Memento<TInterface, TEntity, TVersionedEventPayload, TPayload> : IMemento<TInterface>
    where TEntity : class, TInterface, IEventSourced, new()
    where TVersionedEventPayload : VersionedEventPayload<TPayload>
    where TPayload : notnull
{
    private readonly bool _snapshot;
    private readonly IEventNameResolver _nameResolver;
    private readonly IEventSourcedRepository<TVersionedEventPayload, TPayload> _eventSourcedRepository;
    private readonly Func<IReadOnlyCollection<IMementoEvent<TInterface>>, TEntity>? _factory;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nameResolver"></param>
    /// <param name="eventSourcedRepository"></param>
    /// <param name="factory">Allow personalice the wait you use to build the entity. By the fault the entity will build using the default ctor.</param>
    /// <param name="snapshot"></param>
    public Memento(IEventNameResolver nameResolver,  IEventSourcedRepository<TVersionedEventPayload, TPayload> eventSourcedRepository,  Func<IReadOnlyCollection<IMementoEvent<TInterface>>, TEntity>? factory = null, bool snapshot = false)
    {
        _nameResolver = nameResolver;
        _eventSourcedRepository = eventSourcedRepository;
        _factory = factory;
        _snapshot = snapshot;
    }

    /// <inheritdoc />
    public async Task<TInterface?> FindByVersionAsync(string sourceId, long? version = null, CancellationToken ct = default)
    {
        if (_snapshot)
        {
            var eventPayload = await _eventSourcedRepository.GetAsync(sourceId, version, ct);
            if (eventPayload is null)
                return default;

            return LoadEntityFromSnapshot(eventPayload);
        }

        var eventsPayload = await _eventSourcedRepository.GetHistoryAsync(sourceId, version ?? long.MaxValue, ct);
        if (eventsPayload?.Any() != true)
            return default;
        return LoadEntityFromHistory(eventsPayload);
    }
    /// <inheritdoc />
    public async Task<TInterface?> FindByCreateAsync(string sourceId, DateTime? dateTime = null, CancellationToken ct = default)
    {
        if (_snapshot)
        {
            var eventPayload = await _eventSourcedRepository.GetAsync(sourceId, dateTime, ct);
            if (eventPayload is null)
                return default;

            return LoadEntityFromSnapshot(eventPayload);
        }


        var eventsPayload = await _eventSourcedRepository.GetHistoryAsync(sourceId, dateTime ?? DateTime.MaxValue, ct);
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
    protected virtual IMementoEvent<TInterface> FromEvent(Type type, TPayload payload) => (IMementoEvent<TInterface>)JsonUtility.Deserialize(type, payload.ToString())!;


    #region Nested Classes
    private TInterface LoadEntityFromSnapshot(TVersionedEventPayload eventPayload)
    {
        var eventType = _nameResolver.Resolver(eventPayload.EventName);
        if (eventType is null)
            throw new KeyNotFoundException($"Event name={eventPayload.EventName} can't resolve");
        var @event = FromEvent(eventType, eventPayload.Payload);

        return @event.GetEntity();
    }
    private TEntity LoadEntityFromHistory(TVersionedEventPayload[] eventsPayload)
    {
        var oldEvents = new IMementoEvent<TInterface>[eventsPayload.Length];
        for (var i = 0; i < eventsPayload.Length; i++)
        {
            var eventPayload = eventsPayload[i];
            var eventType = _nameResolver.Resolver(eventPayload.EventName);
            if (eventType is null)
                throw new KeyNotFoundException($"Event name={eventPayload.EventName} can't resolve");

            oldEvents[i] = FromEvent(eventType, eventPayload.Payload);
        }

        var entity = _factory?.Invoke(oldEvents) ?? new TEntity();
        for (var i = 0; i < oldEvents.Length; i++)
            oldEvents[i].Apply(entity);
        return entity;
    }
    #endregion
}