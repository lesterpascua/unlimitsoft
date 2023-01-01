using Azure.Messaging.ServiceBus;
using DotNetMQ.Client;
using UnlimitSoft.EventBus;
using UnlimitSoft.EventBus.Azure;
using UnlimitSoft.EventBus.DotNetMQ;
using UnlimitSoft.WebApi.EventBus.EventBus;

namespace UnlimitSoft.WebApi.EventBus.Processors;


public static class DefaultProcessor
{
    /// <summary>
    /// Event processor
    /// </summary>
    /// <param name="args"></param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask ProcessAsync<TAlias>(ProcessMessageArgs<TAlias, ProcessMessageEventArgs> args, ILogger<AzureEventListener<QueueIdentifier>> logger, CancellationToken ct)
        where TAlias: struct, Enum
    {
        var message = args.Message.Message;
        if (!message.ApplicationProperties.TryGetValue("IdentityId", out var identityId))
            identityId = null;

        logger.LogDebug("Receive from {Queue}, event: {@Event}", args.Queue, args.Envelop);
        try
        {
            logger.LogInformation("Event: {@Args}", args);
            await args.Message.CompleteMessageAsync(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event: {@Args}", args);
            await args.Message.CompleteMessageAsync(message, ct);
        }
    }
    /// <summary>
    /// Event processor
    /// </summary>
    /// <param name="args"></param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask ProcessAsync<TAlias>(ProcessMessageArgs<TAlias, IIncomingMessage> args, ILogger<MemoryEventListener<QueueIdentifier>> logger, CancellationToken ct)
        where TAlias : struct, Enum
    {
        var message = args.Message.MessageData;

        logger.LogDebug("Receive from {Queue}, event: {@Event}", args.Queue, args.Envelop);
        try
        {
            logger.LogInformation("Event: {@Args}", args);
            //await args.Message.CompleteMessageAsync(message, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event: {@Args}", args);
            //await args.Message.CompleteMessageAsync(message, ct);
        }
    }
}
