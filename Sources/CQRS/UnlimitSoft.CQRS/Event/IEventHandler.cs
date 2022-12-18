using UnlimitSoft.Event;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Interfaz for all class for handler event
/// </summary>
public interface IEventHandler : IRequestHandler
{
}
/// <summary>
/// Handle event of generic type.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<in TEvent> : IEventHandler, IRequestHandler<TEvent, IResponse>
    where TEvent : IEvent
{
}
