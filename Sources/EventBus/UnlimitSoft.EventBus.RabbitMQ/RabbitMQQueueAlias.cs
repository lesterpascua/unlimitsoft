using System;
using System.Linq;
using UnlimitSoft.EventBus.Configuration;

namespace UnlimitSoft.EventBus.RabbitMQ;


/// <summary>
/// 
/// </summary>
public class RabbitMQQueueAlias<TAlias> : QueueAlias<TAlias> where TAlias : struct, Enum
{
    /// <summary>
    /// 
    /// </summary>
    public RabbitMQQueueAlias()
    {
        Exchange = string.Empty;
        RoutingKey = Array.Empty<string>();
    }

    /// <summary>
    /// Queue is durable
    /// </summary>
    public bool Durable { get; set; } = true;
    /// <summary>
    /// Adquire exclusive use over the queue
    /// </summary>
    public bool Exclusive { get; set; }
    /// <summary>
    /// Auto delete queue after finish
    /// </summary>
    public bool AutoDelete { get; set; }
    /// <summary>
    /// Exchange use to sent the event
    /// </summary>
    public string Exchange { get; set; }
    /// <summary>
    /// Routing key asociate to the event
    /// </summary>
    public string[] RoutingKey { get; set; }
}
