using System;

namespace SoftUnlimit
{
    /// <summary>
    /// Shared operation for time.
    /// </summary>
    public static class TimeSpanUtility
    {
        /// <summary>
        /// Maximun retry time.
        /// </summary>
        public static readonly TimeSpan MaxRetryTime = TimeSpan.FromMinutes(10);
        /// <summary>
        /// Default initial retry time.
        /// </summary>
        public static readonly TimeSpan InitialRetryTime = TimeSpan.FromSeconds(4);

        /// <summary>
        /// Duplicate the time of retry. 
        /// </summary>
        /// <param name="curr">current time, if null we delay 4 sec the execution by defailt.</param>
        /// <param name="max">If null use the default time 5 min.</param>
        /// <returns></returns>
        public static TimeSpan DuplicateRetryTime(TimeSpan? curr, TimeSpan? max = null)
        {
            var maxTime = max ?? MaxRetryTime;
            var currTime = (curr == null) ? InitialRetryTime : 2 * curr.Value;

            return (currTime < maxTime) ? currTime : maxTime;
        }
    }
}
