using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Event.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class FailSettings
    {
        /// <summary>
        /// Time to wait if exist some fail.
        /// </summary>
        public TimeSpan WaitRetry { get; set; }
    }
}
