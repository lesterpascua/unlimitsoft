using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.Security;

namespace UnlimitSoft.Web.AspNet.Testing;


/// <summary>
/// Fake listener.
/// </summary>
public sealed class ListenerFake : IEventListener
{
    private readonly IJsonSerializer _serializer;
    private readonly IEventDispatcher _eventDispatcher;
    private readonly IEventNameResolver _nameResolver;
    private readonly ILogger<ListenerFake> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="eventDispatcher"></param>
    /// <param name="nameResolver"></param>
    /// <param name="logger"></param>
    public ListenerFake(IJsonSerializer serializer, IEventDispatcher eventDispatcher, IEventNameResolver nameResolver, ILogger<ListenerFake>? logger)
    {
        _serializer = serializer;
        _eventDispatcher = eventDispatcher;
        _nameResolver = nameResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public ValueTask ListenAsync(TimeSpan waitRetry, CancellationToken ct = default) => ValueTask.CompletedTask;

    /// <summary>
    /// Create event with some body.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="serviceMetadata"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static TEvent CreateEvent<TEvent>(IServiceMetadata serviceMetadata, object body)
        where TEvent : class, IEvent
    {
        const long version = 0;
        var @event = (TEvent)Activator.CreateInstance(typeof(TEvent),
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            version,
            serviceMetadata.ServiceId,
            serviceMetadata.WorkerId, 
            Guid.NewGuid().ToString(), 
            null, 
            null, 
            null, 
            false, 
            body
        );
        return @event;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<IResponse> SimulateReceiveAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : class, IEvent
    {
        var eventName = typeof(TEvent).FullName;
        var envelop = new MessageEnvelop(JsonSerializer.Serialize(@event), eventName);
        return await EventUtility.ProcessAsync(
            eventName, 
            envelop, 
            new EventUtility.Args<TEvent>(_serializer, _eventDispatcher, _nameResolver, Logger: _logger),
            ct
        );
    }
}
