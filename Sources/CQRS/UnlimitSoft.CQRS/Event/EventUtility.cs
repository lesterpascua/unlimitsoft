using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Logging;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Event utility
/// </summary>
public static class EventUtility
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="eventName"></param>
    /// <param name="envelop"></param>
    /// <param name="serializer"></param>
    /// <param name="dispatcher"></param>
    /// <param name="resolver">Contains the map between name of the event and event type</param>
    /// <param name="beforeProcess"></param>
    /// <param name="onError"></param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<IResponse?> ProcessAsync<TEvent>(
        string? eventName, 
        MessageEnvelop envelop, 
        IJsonSerializer serializer,
        IEventDispatcher dispatcher, 
        IEventNameResolver resolver, 
        Action<TEvent>? beforeProcess = null, 
        Func<Exception, TEvent?, MessageEnvelop, CancellationToken, ValueTask>? onError = null, 
        ILogger? logger = null, 
        CancellationToken ct = default
    )
        where TEvent : class, IEvent
    {
        TEvent? curr = null;
        IResponse? responses = null;
        try
        {
            Type? eventType = null;
            if (!string.IsNullOrEmpty(envelop.MsgType))
                eventType = resolver.Resolver(envelop.MsgType!);
            if (eventType is null && !string.IsNullOrEmpty(eventName))
                eventType = resolver.Resolver(eventName!);

            if (eventType is null)
            {
                logger?.NoTypeForTheEvent(eventName!);
                return responses;
            }

            // This is alwais deserialized as Json object them convert string a deserialized based of the type
            var json = envelop.Msg.ToString();
            if (serializer.Deserialize(eventType, json) is not TEvent @event)
            {
                logger?.SkipEventType(eventType, eventName!);
                return responses;
            }

            curr = @event;
            beforeProcess?.Invoke(curr!);
            responses = await DispatchEvent(dispatcher, logger, curr!, ct);

            // Log error if fail
            if (responses?.IsSuccess != true)
                throw new EventResponseException("Some event process has error see responses", responses);
        }
        catch (Exception ex)
        {
            logger?.ErrorHandlingEvent(ex, envelop.MsgType, curr?.CorrelationId, envelop.Msg, responses);

            if (onError is not null)
                await onError(ex, curr, envelop, ct);
        }
        return responses;
    }

    private static async ValueTask<IResponse?> DispatchEvent(IEventDispatcher eventDispatcher, ILogger? logger, IEvent @event, CancellationToken ct)
    {
        var (responses, error) = await eventDispatcher.DispatchAsync(@event, ct);
        logger?.LogInformation(@"Procesed
Event: {@Event} 
Response: {@Response}", @event, responses);

        return error ?? responses;
    }
}
