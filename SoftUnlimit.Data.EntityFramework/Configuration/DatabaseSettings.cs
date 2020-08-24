using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.Data.EntityFramework.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// Maximun second amound for retry delay.
        /// </summary>
        public int MaxRetryDelay { get; set; }
        /// <summary>
        /// Maximun attem failed
        /// </summary>
        public int MaxRetryCount { get; set; }
        /// <summary>
        /// Enable sensitive data logging
        /// </summary>
        public bool EnableSensitiveDataLogging { get; set; }
    }
}
