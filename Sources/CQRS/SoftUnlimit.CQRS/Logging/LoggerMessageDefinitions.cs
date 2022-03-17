using Microsoft.Extensions.Logging;
using System;

namespace SoftUnlimit.CQRS.Logging
{
    /// <summary>
    /// Logger definition message for this CQRS assembly
    /// </summary>
    internal static partial class LoggerMessageDefinitions
    {
        [LoggerMessage(
            Message = "Skip event Type: {EventType}, Name: {EventName}",
            Level = LogLevel.Warning,
            SkipEnabledCheck = true
        )] 
        public static partial void NoTypeForTheEvent(this ILogger logger, Type eventType, string eventName);
    }
}