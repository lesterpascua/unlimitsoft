using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        [Obsolete("Use DispatchAsync")]
        Task<CombinedEventResponse> DispatchEventAsync(IEvent @event);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        [Obsolete("Use DispatchAsync")]
        Task<CombinedEventResponse> DispatchEventAsync(IServiceProvider provider, IEvent @event);

        /// <summary>
        /// Dispatch event to asociate with the register event handler.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EventResponse> DispatchAsync(IEvent @event, CancellationToken ct = default);
        /// <summary>
        ///  Dispatch event to asociate with the register event handler in the same scope of the parent.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="event"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EventResponse> DispatchAsync(IServiceProvider provider, IEvent @event, CancellationToken ct = default);
    }
}
