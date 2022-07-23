using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event
{
    public interface IMyEventHandler<TEvent> : IEventHandler<TEvent>
        where TEvent : IEvent
    {
    }
}
