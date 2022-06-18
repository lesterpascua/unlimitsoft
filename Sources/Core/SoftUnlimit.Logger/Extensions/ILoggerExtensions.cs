using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace SoftUnlimit.Logger.Extensions;


/// <summary>
/// Extenssions methods to optimize logger
/// </summary>
public static class ILoggerExtensions
{
    #region Debug
    /// <summary>
    /// Formats and writes an debug log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug<T0>(this ILogger logger, string message, T0? arg0)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(message, args: new object?[] { arg0 });
    }
    /// <summary>
    /// Formats and writes an debug log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug<T0, T1>(this ILogger logger, string message, T0? arg0, T1? arg1)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(message, args: new object?[] { arg0, arg1 });
    }
    /// <summary>
    /// Formats and writes an debug log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug<T0, T1, T2>(this ILogger logger, string message, T0? arg0, T1? arg1, T2? arg2)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(message, args: new object?[] { arg0, arg1, arg2 });
    }
    /// <summary>
    /// Formats and writes an debug log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogDebug<T0, T1, T2, T3>(this ILogger logger, string message, T0? arg0, T1? arg1, T2? arg2, T3? arg3)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            logger.LogDebug(message, args: new object?[] { arg0, arg1, arg2, arg3 });
    }
    #endregion

    #region Information
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation<T0>(this ILogger logger, string message, T0? arg0)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args: new object?[] { arg0 });
    }
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation<T0, T1>(this ILogger logger, string message, T0? arg0, T1? arg1)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args: new object?[] { arg0, arg1 });
    }
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation<T0, T1, T2>(this ILogger logger, string message, T0? arg0, T1? arg1, T2? arg2)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args: new object?[] { arg0, arg1, arg2 });
    }
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogInformation<T0, T1, T2, T3>(this ILogger logger, string message, T0? arg0, T1? arg1, T2? arg2, T3? arg3)
    {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation(message, args: new object?[] { arg0, arg1, arg2, arg3 });
    }
    #endregion

    #region Warning
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning<T0>(this ILogger logger, string message, T0? arg0)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(message, args: new object?[] { arg0 });
    }
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning<T0, T1>(this ILogger logger, string message, T0? arg0, T1? arg1)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(message, args: new object?[] { arg0, arg1 });
    }
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning<T0, T1, T2>(this ILogger logger, string message, T0? arg0, T1? arg1, T2? arg2)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(message, args: new object?[] { arg0, arg1, arg2 });
    }
    /// <summary>
    /// Formats and writes an informational log message
    /// </summary>
    /// <typeparam name="T0"></typeparam>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <typeparam name="T3"></typeparam>
    /// <param name="logger"></param>
    /// <param name="message">Format string of the log message in message template format. Example: "User {User} logged in from {Address}"</param>
    /// <param name="arg0">An object to format</param>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    /// <param name="arg3"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning<T0, T1, T2, T3>(this ILogger logger, string message, T0? arg0, T1? arg1, T2? arg2, T3? arg3)
    {
        if (logger.IsEnabled(LogLevel.Warning))
            logger.LogWarning(message, args: new object?[] { arg0, arg1, arg2, arg3 });
    }
    #endregion
}