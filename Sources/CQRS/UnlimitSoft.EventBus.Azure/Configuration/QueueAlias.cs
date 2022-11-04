using System;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace UnlimitSoft.EventBus.Azure.Configuration;


/// <summary>
/// 
/// </summary>
public class QueueAlias<TAlias> where TAlias : notnull, Enum
{
    /// <summary>
    /// Indicate if the queue is active in this service.
    /// </summary>
    public bool? Active { get; set; }
    /// <summary>
    /// Real name of the queue or topic. By default match with alias
    /// </summary>
    public string Queue { get; set; }
    /// <summary>
    /// Name of the azure subcription if is null will asume is a queue if not <see cref="Queue"/> will interpretate as a topic and use this as subcription
    /// </summary>
    public string? Subscription { get; set; }
    /// <summary>
    /// Alias asociate to the queue.
    /// </summary>
    public TAlias Alias { get; set; }
}
