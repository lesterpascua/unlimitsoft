using SoftUnlimit.CQRS.Message;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventDispatcherWithServiceProvider : IEventDispatcher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        Task<CombinedEventResponse> DispatchEventAsync(IServiceProvider provider, IEvent @event);
    }
}
