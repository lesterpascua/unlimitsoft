using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Event;

namespace SoftUnlimit.CQRS.EventSourcing
{
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
        public VersionedEventPayload(IVersionedEvent @event)
            : base(@event)
        {
            Version = @event.Version;
        }

        /// <summary>
        /// 
        /// </summary>
        public long Version { get; set; }
    }
}
