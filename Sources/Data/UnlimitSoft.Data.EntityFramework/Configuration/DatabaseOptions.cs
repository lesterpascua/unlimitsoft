namespace UnlimitSoft.Data.EntityFramework.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class DatabaseOptions
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
