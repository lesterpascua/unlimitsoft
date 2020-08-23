using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publis event in bus.
        /// </summary>
        /// <param name="eventPayload"></param>
        /// <returns></returns>
        Task PublishAsync(VersionedEventPayload eventPayload);
    }
}
