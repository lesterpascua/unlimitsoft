using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.Event.Json;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.CQRS.Message;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.EventBus.Azure
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcessorUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="resolver"></param>
        /// <param name="envelop"></param>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <param name="beforeProcess">Allow to update event before process. If this function return true raise the error </param>
        /// <param name="onError"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public static async Task<(IEventResponse, Exception)> Default<TEvent>(
            IEventDispatcher dispatcher,
            IEventNameResolver resolver,
            MessageEnvelop envelop,
            ServiceBusReceivedMessage message,
            Action<TEvent> beforeProcess = null,
            Func<Exception, TEvent, MessageEnvelop, CancellationToken, Task> onError = null,
            ILogger logger = null,
            CancellationToken ct = default
        )
            where TEvent : class, IEvent
        {
            message.ApplicationProperties.TryGetValue(BusProperty.EventName, out var eventName);
            message.ApplicationProperties.TryGetValue(BusProperty.MessageType, out var messageType);

            return await EventUtility.Process(eventName?.ToString(), envelop, dispatcher, resolver, beforeProcess, onError, logger, ct);
        }
    }
}
