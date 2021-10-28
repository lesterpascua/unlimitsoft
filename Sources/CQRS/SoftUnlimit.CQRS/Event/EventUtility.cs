using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using SoftUnlimit.Web.Event;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
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
        /// <param name="resolver"></param>
        /// <param name="beforeProcess"></param>
        /// <param name="onError"></param>
        /// <param name="logger"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<(IEventResponse, Exception)> ProcessAsync<TEvent>(
            string eventName, MessageEnvelop envelop, IEventDispatcher dispatcher, IEventNameResolver resolver,
            Action<TEvent> beforeProcess = null, Func<Exception, TEvent, MessageEnvelop, CancellationToken, Task> onError = null, ILogger logger = null, CancellationToken ct = default
        )
            where TEvent : class, IEvent
        {
            TEvent curr = null;
            Exception ex = null;
            IEventResponse responses = null;
            try
            {
                Type eventType = null;
                if (!string.IsNullOrEmpty(envelop.MessajeType))
                    eventType = resolver.Resolver(envelop.MessajeType);
                if (eventType is null && !string.IsNullOrEmpty(eventName))
                    eventType = resolver.Resolver(eventName);

                if (eventType is null)
                {
                    logger?.LogWarning("Skip event Type: {EventType}, Name: {EventName}", eventType, eventName);
                    return (responses, ex);
                }

                var payload = envelop.Messaje.ToString();
                if (JsonUtility.Deserialize(eventType, payload) is TEvent @event)
                {
                    curr = @event;
                    beforeProcess?.Invoke(@event);
                    responses = await DispatchEvent(dispatcher, logger, @event, ct);
                }
                else
                    logger?.LogWarning("Skip event Type: {EventType}, Name: {EventName} don't meet the requirement", eventType, eventName);

                if (responses?.IsSuccess != true)
                    throw new EventResponseException("Some event process has error see responses", responses);
            }
            catch (Exception e)
            {
                ex = e;
                logger?.LogError(ex, "Error handling event {Type}, payload: {Event}, {@Response}", envelop.MessajeType, envelop.Messaje, responses);

                if (onError != null)
                    await onError(ex, curr, envelop, ct);
            }
            return (responses, ex);
        }

        private static async Task<IEventResponse> DispatchEvent(IEventDispatcher eventDispatcher, ILogger logger, IEvent @event, CancellationToken ct)
        {
            var responses = await eventDispatcher.DispatchAsync(@event, ct);
            logger?.LogInformation(
@$"Procesed
Event: {{@Event}}
Response: {{@Response}}", @event, responses);

            return responses;
        }
    }
}
