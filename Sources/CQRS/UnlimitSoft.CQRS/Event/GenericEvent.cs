using UnlimitSoft.Event;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Generic versioned event use to deserialize any kind of event.
/// </summary>
public sealed class GenericEvent : Event<object>
{
}
