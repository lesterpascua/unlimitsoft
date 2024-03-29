﻿using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Logging;
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
    /// <param name="args"></param>
    /// <param name="extraArgs">Extra parameter required to supplied to on error function</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<IResponse?> ProcessAsync<TEvent>(string? eventName, MessageEnvelop envelop, Args<TEvent> args, object? extraArgs, CancellationToken ct = default) where TEvent : class, IEvent
    {
        TEvent? curr = null;
        IResponse? responses = null;
        try
        {
            Type? eventType = null;
            if (!string.IsNullOrEmpty(envelop.MsgType))
                eventType = args.Resolver.Resolver(envelop.MsgType!);
            if (eventType is null && !string.IsNullOrEmpty(eventName))
                eventType = args.Resolver.Resolver(eventName!);

            if (eventType is null)
            {
                args.Logger?.NoTypeForTheEvent(eventName!);
                return responses;
            }

            // This is alwais deserialized as Json object them convert string a deserialized based of the type
            var json = envelop.Msg.ToString();
            if (args.Serializer.Deserialize(eventType, json) is not TEvent @event)
            {
                args.Logger?.SkipEventType(eventType, eventName!);
                return responses;
            }

            curr = @event;
            args.BeforeProcess?.Invoke(curr!, extraArgs);
            responses = await DispatchEvent(args.Dispatcher, args.Logger, curr!, ct);

            // Log error if fail
            if (responses?.IsSuccess != true)
            {
                if (responses?.Code != HttpStatusCode.NotFound || !args.NotFoundAsWarning)
                    throw new EventResponseException("Some event process has error see responses", responses);

                args.Logger?.LogWarning("Event: {Name} don't have handler associate", eventType.FullName);
            }
        }
        catch (Exception ex)
        {
            args.Logger?.ErrorHandlingEvent(ex, envelop.MsgType, curr?.CorrelationId, envelop.Msg, responses);

            if (args.OnError is not null)
                await args.OnError(ex, curr, envelop, extraArgs, ct);
        }
        return responses;
    }

    #region Private Methods
    private static async ValueTask<IResponse?> DispatchEvent(IEventDispatcher eventDispatcher, ILogger? logger, IEvent @event, CancellationToken ct)
    {
        var (responses, error) = await eventDispatcher.DispatchAsync(@event, ct);
        logger?.LogInformation(@"Procesed
Event: {@Event} 
Response: {@Response}", @event, responses);

        return error ?? responses;
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="Serializer"></param>
    /// <param name="Dispatcher"></param>
    /// <param name="Resolver">Contains the map between name of the event and event type</param>
    /// <param name="BeforeProcess"></param>
    /// <param name="OnError"></param>
    /// <param name="NotFoundAsWarning">If no handler attached just log a warning don't thread as an error</param>
    /// <param name="Logger"></param>
    public sealed record Args<TEvent>(IJsonSerializer Serializer, IEventDispatcher Dispatcher, IEventNameResolver Resolver, Action<TEvent, object?>? BeforeProcess = null, Func<Exception, TEvent?, MessageEnvelop, object?, CancellationToken, ValueTask>? OnError = null, bool NotFoundAsWarning = true, ILogger? Logger = null);
    #endregion
}
