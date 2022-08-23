using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Event;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.EventBus.Azure;


/// <summary>
/// Funtion used to process the event
/// </summary>
/// <param name="args"></param>
/// <param name="ct"></param>
/// <returns></returns>
public delegate Task ProcessorCallback(ProcessMessageArgs args, CancellationToken ct);
/// <summary>
/// Arguments comming from bus
/// </summary>
/// <param name="Queue"></param>
/// <param name="Envelop"></param>
/// <param name="Azure"></param>
/// <param name="WaitRetry"></param>
/// <param name="Logger"></param>
public record ProcessMessageArgs(string Queue, MessageEnvelop Envelop, ProcessMessageEventArgs Azure, TimeSpan WaitRetry, ILogger Logger);