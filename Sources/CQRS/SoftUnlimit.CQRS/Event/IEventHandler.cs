using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Event;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// Interfaz for all class for handler event
    /// </summary>
    public interface IEventHandler
    {
    }
    /// <summary>
    /// Handle event of generic type.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> : IEventHandler where TEvent : IEvent
    {
        /// <summary>
        /// Handler a event
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IEventResponse> HandleAsync(TEvent @event, CancellationToken ct = default);
    }
}
