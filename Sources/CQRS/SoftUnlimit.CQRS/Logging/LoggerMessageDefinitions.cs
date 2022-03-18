using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using System;

namespace SoftUnlimit.CQRS.Logging
{
    /// <summary>
    /// Logger definition message for this CQRS assembly
    /// </summary>
    internal static partial class LoggerMessageDefinitions
    {
        #region EventUtility
        private static readonly Action<ILogger, string, string, object, IEventResponse, Exception?> __ErrorHandlingEvent = LoggerMessage.Define<string, string, object, IEventResponse>(LogLevel.Error, 0, "Error handling event {Type}, {CorrelationId} payload: {Event}, {@Response}", new LogDefineOptions { SkipEnabledCheck = true });

        [LoggerMessage(Message = "Skip event Type: {EventType}, Name: {EventName}", Level = LogLevel.Warning, SkipEnabledCheck = true)]
        public static partial void NoTypeForTheEvent(this ILogger logger, Type eventType, string eventName);
        [LoggerMessage(Message = "Skip event Type: {EventType}, Name: {EventName} don't meet the requirement", Level = LogLevel.Warning, SkipEnabledCheck = true)]
        public static partial void SkipEventType(this ILogger logger, Type eventType, string eventName);
        public static void ErrorHandlingEvent(this ILogger logger, Exception ex, string type, string correlation, object msg, IEventResponse response) => __ErrorHandlingEvent(logger, type, correlation, msg, response, ex);
        #endregion

        #region ServiceProviderCommandDispatcher
        private static readonly Action<ILogger, ICommand, Exception?> __ProcessCommand = LoggerMessage.Define<ICommand>(LogLevel.Debug, 0, "Process command: {@Command}");
        private static readonly Action<ILogger, ValidationResult, Exception?> __EvaluateValidatorProcessResultErrors = LoggerMessage.Define<ValidationResult>(LogLevel.Debug, 0, "Evaluate validator process result: {@Errors}");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        public static void ProcessCommand(this ILogger logger, ICommand command) => __ProcessCommand(logger, command, null);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="type"></param>
        [LoggerMessage(Message = "Execute command type: {Type}", Level = LogLevel.Debug)]
        public static partial void ExecuteCommandType(this ILogger logger, Type type);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        [LoggerMessage(Message = "Command {Command} not handler implement internal compliance", Level = LogLevel.Debug)]
        public static partial void CommandNotHandlerImplementCompliance(this ILogger logger, ICommand command);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        [LoggerMessage(Message = "Command {Command} handler implement internal compliance", Level = LogLevel.Debug)]
        public static partial void CommandHandlerImplementCompliance(this ILogger logger, ICommand command);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        [LoggerMessage(Message = "Command {Command} not handler implement internal validation", Level = LogLevel.Debug)]
        public static partial void CommandNotHandlerImplementValidation(this ILogger logger, ICommand command);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="command"></param>
        [LoggerMessage(Message = "Command {Command} handler implement internal validation", Level = LogLevel.Debug)]
        public static partial void CommandHandlerImplementValidation(this ILogger logger, ICommand command);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="errors"></param>
        public static void EvaluateValidatorProcessResultErrors(this ILogger logger, ValidationResult errors) => __EvaluateValidatorProcessResultErrors(logger, errors, null);
        #endregion
    }
}