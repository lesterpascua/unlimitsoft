using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.WebApi.EventBus.EventBus;


public sealed class EventBusOptions : EventBusOptions<QueueIdentifier>
{
    /// <summary>
    /// 
    /// </summary>
    public string Endpoint { get; set; } = default!;
}
