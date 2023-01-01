using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.EventBus.Azure;


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
    /// <param name="serializer"></param>
    /// <param name="envelop"></param>
    /// <param name="message"></param>
    /// <param name="logger"></param>
    /// <param name="beforeProcess">Allow to update event before process. If this function return true raise the error </param>
    /// <param name="onError"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<IResponse?> Default<TEvent>(
        IEventDispatcher dispatcher,
        IEventNameResolver resolver,
        IJsonSerializer serializer,
        MessageEnvelop envelop,
        ServiceBusReceivedMessage message,
        Action<TEvent>? beforeProcess = null,
        Func<Exception, TEvent?, MessageEnvelop, CancellationToken, ValueTask>? onError = null,
        ILogger? logger = null,
        CancellationToken ct = default
    )
        where TEvent : class, IEvent
    {
        message.ApplicationProperties.TryGetValue(BusProperty.EventName, out var eventName);
        message.ApplicationProperties.TryGetValue(BusProperty.MessageType, out var messageType);

        return await EventUtility.ProcessAsync(
            eventName?.ToString(), 
            envelop, 
            serializer,
            dispatcher, 
            resolver, 
            beforeProcess, 
            onError, 
            logger, 
            ct
        );
    }
}
