using Azure.Messaging.ServiceBus;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Event;
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
    /// <param name="envelop"></param>
    /// <param name="message"></param>
    /// <param name="args"></param>
    /// <param name="extraArgs">Extra parameter required to supplied to on error function</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<IResponse?> Default<TEvent>(MessageEnvelop envelop, ServiceBusReceivedMessage message, EventUtility.Args<TEvent> args, object? extraArgs, CancellationToken ct = default)
        where TEvent : class, IEvent
    {
        message.ApplicationProperties.TryGetValue(Constants.HeaderEventName, out var eventName);

        return await EventUtility.ProcessAsync(eventName?.ToString(), envelop, args, extraArgs, ct);
    }
}
