using Hangfire.Logging;
using System;

namespace UnlimitSoft.Bus.Hangfire
{
    /// <summary>
    /// 
    /// </summary>
    public class HangfireOptions
    {
        /// <summary>
        /// Database scheme
        /// </summary>
        public string Scheme { get; set; }
        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan SchedulePollingInterval { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int WorkerCount { get; set; }
        /// <summary>
        /// Hang fire logger.
        /// </summary>
        public LogLevel Logger { get; set; }
    }
}
