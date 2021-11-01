using SoftUnlimit.Event;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Generic event use to deserialize any kind of event.
    /// </summary>
    public sealed class GenericEvent : Event<object, object>
    {
    }
}
