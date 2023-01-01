using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.EventBus;


/// <summary>
/// Funtion used to process the event
/// </summary>
/// <typeparam name="TAlias"></typeparam>
/// <typeparam name="TMessage"></typeparam>
/// <param name="args"></param>
/// <param name="ct"></param>
/// <returns></returns>
public delegate ValueTask ProcessorCallback<TAlias, TMessage>(ProcessMessageArgs<TAlias, TMessage> args, CancellationToken ct) where TAlias : struct, Enum;
/// <summary>
/// Arguments comming from bus
/// </summary>
/// <typeparam name="TAlias"></typeparam>
/// <typeparam name="TMessage"></typeparam>
/// <param name="Queue"></param>
/// <param name="Envelop"></param>
/// <param name="Message"></param>
/// <param name="WaitRetry"></param>
public record ProcessMessageArgs<TAlias, TMessage>(QueueAlias<TAlias> Queue, MessageEnvelop Envelop, TMessage Message, TimeSpan WaitRetry) where TAlias : struct, Enum;