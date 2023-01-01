using System;

namespace UnlimitSoft.EventBus.DotNetMQ.Configuration;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public sealed class EventBusOptions<TAlias> : EventBus.Configuration.EventBusOptions<TAlias>
    where TAlias : struct, Enum
{
    /// <summary>
    /// Name of the bus.
    /// </summary>
    public string Name { get; set; } = default!;
}
