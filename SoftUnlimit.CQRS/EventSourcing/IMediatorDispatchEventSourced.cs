using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.EventSourcing
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMediatorDispatchEventSourced
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task DispatchEventsAsync(IEnumerable<IVersionedEvent> entities);
    }
}
