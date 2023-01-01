using System;

namespace UnlimitSoft.EventBus.Configuration;


/// <summary>
/// Allow definition of an queue and the alias
/// </summary>
public class QueueAlias<TAlias> where TAlias : struct, Enum
{
    /// <summary>
    /// Indicate if the queue is active in this service.
    /// </summary>
    public bool? Active { get; set; }
    /// <summary>
    /// Real name of the queue or topic. By default match with alias
    /// </summary>
    public string Queue { get; set; } = default!;
    /// <summary>
    /// Name of the azure subcription if is null will asume is a queue if not <see cref="Queue"/> will interpretate as a topic and use this as subcription
    /// </summary>
    public string? Subscription { get; set; }
    /// <summary>
    /// Alias asociate to the queue. Alias is usefull to point to diferent queue real name. Depending of the environment.
    /// For DEV environment the queue could names queue-dev and for QA envitonment could named queue-qa in the 
    /// code we alwais will be pointing to the alias name and the real quueue name will keep hide for the implementation
    /// </summary>
    public TAlias Alias { get; set; } = default!;
}
