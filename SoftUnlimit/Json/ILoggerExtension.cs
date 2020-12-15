using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SoftUnlimit.Json
{
    /// <summary>
    /// 
    /// </summary>
    public static class ILoggerExtension
    {
        /// <summary>
        /// Write message idented.
        /// </summary>
        public static bool Indented = false;
        private static readonly EventId Empty = new EventId();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="data"></param>
        /// <param name="eventId"></param>
        public static void LogTraceJson(this ILogger logger, object data, EventId? eventId = null) => logger.LogTrace(eventId ?? Empty, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = Indented }));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="data"></param>
        /// <param name="eventId"></param>
        public static void LogDebugJson(this ILogger logger, object data, EventId? eventId = null) => logger.LogDebug(eventId ?? Empty, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = Indented }));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="data"></param>
        /// <param name="eventId"></param>
        public static void LogInformationJson(this ILogger logger, object data, EventId? eventId = null) => logger.LogInformation(eventId ?? Empty, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = Indented }));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="data"></param>
        /// <param name="eventId"></param>
        public static void LogWarningJson(this ILogger logger, object data, EventId? eventId = null) => logger.LogWarning(eventId ?? Empty, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = Indented }));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="data"></param>
        /// <param name="exception"></param>
        /// <param name="eventId"></param>
        public static void LogErrorJson(this ILogger logger, object data, Exception exception, EventId? eventId = null) => logger.LogError(eventId ?? Empty, exception, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = Indented }));
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="data"></param>
        /// <param name="exception"></param>
        /// <param name="eventId"></param>
        public static void LogCriticalJson(this ILogger logger, object data, Exception exception, EventId? eventId = null) => logger.LogCritical(eventId ?? Empty, exception, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = Indented }));
    }
}
