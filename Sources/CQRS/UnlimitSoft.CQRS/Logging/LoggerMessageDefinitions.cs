using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using System;

#pragma warning disable IDE0060 // Remove unused parameter
namespace UnlimitSoft.CQRS.Logging;

/// <summary>
/// Logger definition message for this CQRS assembly
/// </summary>
internal static partial class LoggerMessageDefinitions
{
    #region EventUtility
    private static readonly Action<ILogger, string, string, object, IEventResponse, Exception?> __ErrorHandlingEvent = LoggerMessage.Define<string, string, object, IEventResponse>(LogLevel.Error, 0, "Error handling event {Type}, {CorrelationId} payload: {Event}, {@Response}", new LogDefineOptions { SkipEnabledCheck = true });

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="ex"></param>
    /// <param name="type"></param>
    /// <param name="correlation"></param>
    /// <param name="msg"></param>
    /// <param name="response"></param>
    public static void ErrorHandlingEvent(this ILogger logger, Exception ex, string type, string correlation, object msg, IEventResponse response) => __ErrorHandlingEvent(logger, type, correlation, msg, response, ex);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="eventType"></param>
    /// <param name="eventName"></param>
    [LoggerMessage(EventId = 1, Message = "Skip event Type: {EventType}, Name: {EventName}", Level = LogLevel.Warning, SkipEnabledCheck = true)]
    public static partial void NoTypeForTheEvent(this ILogger logger, Type eventType, string eventName);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="eventType"></param>
    /// <param name="eventName"></param>
    [LoggerMessage(EventId = 2, Message = "Skip event Type: {EventType}, Name: {EventName} don't meet the requirement", Level = LogLevel.Warning, SkipEnabledCheck = true)]
    public static partial void SkipEventType(this ILogger logger, Type eventType, string eventName);
    #endregion

    #region ServiceProviderCommandDispatcher
    private static readonly Action<ILogger, ICommand, Exception?> __ServiceProviderCommandDispatcher_ProcessCommand = LoggerMessage.Define<ICommand>(LogLevel.Debug, 10, "Process command: {@Command}");
    private static readonly Action<ILogger, ValidationResult, Exception?> __ServiceProviderCommandDispatcher_EvaluateValidatorProcessResultErrors = LoggerMessage.Define<ValidationResult>(LogLevel.Debug, 11, "Evaluate validator process result: {@Errors}");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="command"></param>
    public static void ServiceProviderCommandDispatcher_ProcessCommand(this ILogger logger, ICommand command) => __ServiceProviderCommandDispatcher_ProcessCommand(logger, command, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="errors"></param>
    public static void ServiceProviderCommandDispatcher_EvaluateValidatorProcessResultErrors(this ILogger logger, ValidationResult errors) => __ServiceProviderCommandDispatcher_EvaluateValidatorProcessResultErrors(logger, errors, null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="type"></param>
    [LoggerMessage(EventId = 12, Message = "Execute command type: {Type}", Level = LogLevel.Debug)]
    public static partial void ServiceProviderCommandDispatcher_ExecuteCommandType(this ILogger logger, Type type);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="command"></param>
    [LoggerMessage(EventId = 13, Message = "Command {Command} not handler implement internal compliance", Level = LogLevel.Debug)]
    public static partial void ServiceProviderCommandDispatcher_CommandNotHandlerImplementCompliance(this ILogger logger, ICommand command);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="command"></param>
    [LoggerMessage(EventId = 14, Message = "Command {Command} handler implement internal compliance", Level = LogLevel.Debug)]
    public static partial void ServiceProviderCommandDispatcher_CommandHandlerImplementCompliance(this ILogger logger, ICommand command);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="command"></param>
    [LoggerMessage(EventId = 15, Message = "Command {Command} not handler implement internal validation", Level = LogLevel.Debug)]
    public static partial void ServiceProviderCommandDispatcher_CommandNotHandlerImplementValidation(this ILogger logger, ICommand command);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="command"></param>
    [LoggerMessage(EventId = 16, Message = "Command {Command} handler implement internal validation", Level = LogLevel.Debug)]
    public static partial void ServiceProviderCommandDispatcher_CommandHandlerImplementValidation(this ILogger logger, ICommand command);
    #endregion

    #region ServiceProviderQueryDispatcher
    private static readonly Action<ILogger, IQuery, Exception?> __ServiceProviderQueryDispatcher_ProcessCommand = LoggerMessage.Define<IQuery>(LogLevel.Debug, 20, "Process query: {@Query}");
    private static readonly Action<ILogger, ValidationResult, Exception?> __ServiceProviderQueryDispatcher_EvaluateValidatorProcessResultErrors = LoggerMessage.Define<ValidationResult>(LogLevel.Debug, 21, "Evaluate validator process result: {@Errors}");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="query"></param>
    public static void ServiceProviderQueryDispatcher_ProcessQuery(this ILogger logger, IQuery query) => __ServiceProviderQueryDispatcher_ProcessCommand(logger, query, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="errors"></param>
    public static void ServiceProviderQueryDispatcher_EvaluateValidatorProcessResultErrors(this ILogger logger, ValidationResult errors) => __ServiceProviderQueryDispatcher_EvaluateValidatorProcessResultErrors(logger, errors, null);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="type"></param>
    [LoggerMessage(EventId = 22, Message = "Execute query type: {Type}", Level = LogLevel.Debug)]
    public static partial void ServiceProviderQueryDispatcher_ExecuteCommandType(this ILogger logger, Type type);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="query"></param>
    [LoggerMessage(EventId = 23, Message = "Query {Query} not handler implement internal compliance", Level = LogLevel.Debug)]
    public static partial void ServiceProviderQueryDispatcher_QueryNotHandlerImplementCompliance(this ILogger logger, IQuery query);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="query"></param>
    [LoggerMessage(EventId = 24, Message = "Query {Query} handler implement internal compliance", Level = LogLevel.Debug)]
    public static partial void ServiceProviderQueryDispatcher_QueryHandlerImplementCompliance(this ILogger logger, IQuery query);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="query"></param>
    [LoggerMessage(EventId = 25, Message = "Query {Query} not handler implement internal validation", Level = LogLevel.Debug)]
    public static partial void ServiceProviderQueryDispatcher_QueryNotHandlerImplementValidation(this ILogger logger, IQuery query);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="query"></param>
    [LoggerMessage(EventId = 26, Message = "Query {Query} handler implement internal validation", Level = LogLevel.Debug)]
    public static partial void ServiceProviderQueryDispatcher_QueryHandlerImplementValidation(this ILogger logger, IQuery query);
    #endregion
}