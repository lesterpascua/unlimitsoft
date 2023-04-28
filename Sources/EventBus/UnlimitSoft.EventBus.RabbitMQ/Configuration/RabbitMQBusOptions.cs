using System;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.EventBus.RabbitMQ.Configuration;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TAlias"></typeparam>
public sealed class RabbitMQBusOptions<TAlias> : EventBusOptions<RabbitMQQueueAlias<TAlias>, TAlias> where TAlias : struct, Enum
{
    /// <summary>
    /// 
    /// </summary>
    public required string Endpoint { get; set; }
}