using UnlimitSoft.EventBus.Azure;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.WebApi.EventBus.EventBus;


public sealed class EventBusOptions : EventBusOptions<AzureQueueAlias<QueueIdentifier>, QueueIdentifier>
{
    /// <summary>
    /// 
    /// </summary>
    public string Endpoint { get; set; } = default!;
}
