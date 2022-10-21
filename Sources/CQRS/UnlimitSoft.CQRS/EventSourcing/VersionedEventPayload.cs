using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;

namespace UnlimitSoft.CQRS.EventSourcing;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public abstract class VersionedEventPayload<TPayload> : EventPayload<TPayload>
{
    /// <summary>
    /// 
    /// </summary>
    public VersionedEventPayload()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="payload"></param>
    public VersionedEventPayload(IVersionedEvent @event, TPayload payload)
        : base(@event, payload)
    {
        Version = @event.Version;
    }

    /// <summary>
    /// 
    /// </summary>
    public long Version { get; set; }
}
