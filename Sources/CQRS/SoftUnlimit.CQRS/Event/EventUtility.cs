using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using SoftUnlimit.Event;
using System;
using System.Threading;
using System.Threading.Tasks;
using SoftUnlimit.CQRS.Logging;

namespace SoftUnlimit.CQRS.Event;


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
    /// <param name="dispatcher"></param>
    /// <param name="resolver">Contains the map between name of the event and event type</param>
    /// <param name="beforeProcess"></param>
    /// <param name="onError"></param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<(IEventResponse, Exception)> ProcessAsync<TEvent>(string eventName, MessageEnvelop envelop, IEventDispatcher dispatcher, IEventNameResolver resolver, Action<TEvent> beforeProcess = null, Func<Exception, TEvent, MessageEnvelop, CancellationToken, Task> onError = null, ILogger logger = null, CancellationToken ct = default)
        where TEvent : class, IEvent
    {
        TEvent curr = null;
        Exception ex = null;
        IEventResponse responses = null;
        try
        {
            Type eventType = null;
            if (!string.IsNullOrEmpty(envelop.MsgType))
                eventType = resolver.Resolver(envelop.MsgType);
            if (eventType is null && !string.IsNullOrEmpty(eventName))
                eventType = resolver.Resolver(eventName);

            if (eventType is null)
            {
                logger?.NoTypeForTheEvent(eventType, eventName);
                return (responses, ex);
            }

            switch (envelop.Type)
            {
                case MessageType.Json:
                    var json = envelop.Msg.ToString();
                    if (JsonUtility.Deserialize(eventType, json) is not TEvent @event)
                    {
                        logger?.SkipEventType(eventType, eventName);
                        break;
                    }
                    curr = @event;
                    break;
                case MessageType.Event:
                    curr = (TEvent)envelop.Msg;
                    break;
                default:
                    throw new NotSupportedException($"Message type {envelop.Type} is not suported");
            }
            beforeProcess?.Invoke(curr);
            responses = await DispatchEvent(dispatcher, logger, curr, ct);

            // Log error if fail
            if (responses?.IsSuccess != true)
                throw new EventResponseException("Some event process has error see responses", responses);
        }
        catch (Exception e)
        {
            ex = e;
            logger?.ErrorHandlingEvent(ex, envelop.MsgType, curr?.CorrelationId, envelop.Msg, responses);

            if (onError is not null)
                await onError(ex, curr, envelop, ct);
        }
        return (responses, ex);
    }

    private static async Task<IEventResponse> DispatchEvent(IEventDispatcher eventDispatcher, ILogger logger, IEvent @event, CancellationToken ct)
    {
        var responses = await eventDispatcher.DispatchAsync(@event, ct);
        logger?.LogInformation(@"Procesed
Event: {@Event} 
Response: {@Response}", @event, responses);

        return responses;
    }
}
