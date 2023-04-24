using System;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.EventBus.Azure;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public class AzureQueueAlias<TAlias> : QueueAlias<TAlias> where TAlias : struct, Enum
{
    /// <summary>
    /// Name of the azure subcription if is null will asume is a queue if not <see cref="Queue"/> will interpretate as a topic and use this as subcription
    /// </summary>
    public string? Subscription { get; set; }
}
