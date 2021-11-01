using SoftUnlimit.Event;
using System;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Allow set some time to delay the event.
    /// </summary>
    /// <remarks>Some brokered message don't support this feature, use with carefull.</remarks>
    public interface IDelayEvent : IEvent
    {
        /// <summary>
        /// Get Delay time of the event.
        /// </summary>
        TimeSpan? GetDelay();
        /// <summary>
        /// Set Delay time of the event.
        /// </summary>
        /// <param name="delay"></param>
        void SetDelay(TimeSpan delay);
    }
}
