using System;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.EventBus.Azure.Configuration;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public sealed class AzureEventBusOptions<TAlias> : EventBusOptions<AzureQueueAlias<TAlias>, TAlias>
    where TAlias : struct, Enum
{
    /// <summary>
    /// Azure endpoint connection string.
    /// </summary>
    public string Endpoint { get; set; } = default!;
}
