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
        /// Enter a background process for listener loop of event.
        /// </summary>
        void Listen();
    }
}
