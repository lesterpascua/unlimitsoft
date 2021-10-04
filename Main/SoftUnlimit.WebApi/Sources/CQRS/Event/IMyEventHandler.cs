using SoftUnlimit.CQRS.Event;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    public interface IMyEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
    }
}
