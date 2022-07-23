using System;

namespace UnlimitSoft
{
    /// <summary>
    /// Date time helper methods.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Try to convert the date into UTC depending of the type specified.
        /// Unspecified: assumen is UTC,
        /// Local: Convertion using the current time zone,
        /// UTC: return same
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ToSafeUtc(this DateTime date)
        {
            date = date.Kind switch
            {
                DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
                DateTimeKind.Local => date.ToUniversalTime(),
                _ => date
            };
            return date;
        }
        /// <summary>
        /// Try to convert the date into UTC depending of the type specified.
        /// Unspecified: assumen is UTC,
        /// Local: Convertion using the current time zone,
        /// UTC: return same
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime? ToSafeUtc(this DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToSafeUtc();
            return null;
        }
    }
}
