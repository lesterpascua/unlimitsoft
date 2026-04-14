using Azure.Messaging.ServiceBus.Administration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.EventBus.Azure;


/// <summary>
/// Provides helper methods for creating and managing Azure Service Bus topics and subscriptions.
/// </summary>
public static class TopicHelper
{
    /// <summary>
    /// Checks if the specified topic, queue, and subscription exist in Azure Service Bus, and creates them if they do not exist.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="queue"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task CreateAsync(ServiceBusAdministrationClient client, TopicQueueBind queue, CancellationToken ct = default)
    {
        var existTopicResponse = await client.TopicExistsAsync(queue.EventName, ct);
        if (!existTopicResponse.Value)
            await client.CreateTopicAsync(queue.EventName, ct);

        var queueResponse = await client.QueueExistsAsync(queue.Queue, ct);
        if (!queueResponse.Value)
        {
            var createQueueOptions = new CreateQueueOptions(queue.Queue);
            await client.CreateQueueAsync(createQueueOptions, ct);
        }

        var existSubcriptionResponse = await client.SubscriptionExistsAsync(queue.EventName, queue.Subscription, ct);
        if (!existSubcriptionResponse.Value)
        {
            var createSubscriptionOptions = new CreateSubscriptionOptions(queue.EventName, queue.Subscription) { ForwardTo = queue.Queue };
            await client.CreateSubscriptionAsync(createSubscriptionOptions, ct);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="queue"></param>
    /// <param name="eventType"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task CreateAsync<TAlias>(ServiceBusAdministrationClient client, AzureQueueAlias<TAlias> queue, Type eventType, CancellationToken ct = default)
        where TAlias : struct, Enum
    {
        var bind = new TopicQueueBind(eventType.FullName!, queue.Queue, queue.Subscription!);
        return CreateAsync(client, bind, ct);
    }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    /// <param name="EventName"></param>
    /// <param name="Queue"></param>
    /// <param name="Subscription"></param>
    public sealed record TopicQueueBind(string EventName, string Queue, string Subscription);
    #endregion
}
