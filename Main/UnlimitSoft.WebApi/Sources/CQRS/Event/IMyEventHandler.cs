using SoftUnlimit.CQRS.Event;
using SoftUnlimit.Event;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public interface IMyEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
    }
}
