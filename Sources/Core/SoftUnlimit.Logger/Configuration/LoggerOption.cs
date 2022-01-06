using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace SoftUnlimit.Logger.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LoggerOption
    {
        /// <summary>
        /// 
        /// </summary>
        public LogLevel Default { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, LogLevel>? Override { get; set; }
    }
}
