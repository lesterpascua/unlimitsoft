using System;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Allow set some time to delay the event.
/// </summary>
/// <remarks>Some brokered message don't support this feature, use with carefull.</remarks>
public interface IDelayEvent : IEvent
{
    /// <summary>
    /// Time when the event has to be trigger.
    /// </summary>
    DateTime? Scheduled { get; set; }
}
