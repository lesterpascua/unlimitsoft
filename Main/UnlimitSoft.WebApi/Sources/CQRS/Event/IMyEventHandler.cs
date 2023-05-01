using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Event;


public interface IMyEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IEvent
{
}
