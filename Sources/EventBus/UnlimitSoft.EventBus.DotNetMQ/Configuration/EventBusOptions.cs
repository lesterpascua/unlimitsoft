using System;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.EventBus.DotNetMQ.Configuration;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public sealed class EventBusOptions<TAlias> : EventBusOptions<QueueAlias<TAlias>, TAlias>
    where TAlias : struct, Enum
{
    /// <summary>
    /// Name of the bus.
    /// </summary>
    public string Name { get; set; } = default!;
}
