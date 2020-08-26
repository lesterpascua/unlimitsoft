using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        /// Enter a background process for listener loop of event. If connection fail should recover.
        /// </summary>
        /// <param name="waitRetry">If fail indicate time to wait until retry again.</param>
        void Listen(TimeSpan waitRetry);
    }
}
