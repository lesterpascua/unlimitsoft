using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Threading;

namespace UnlimitSoft.CQRS.Threading;


/// <summary>
/// 
/// </summary>
public static class TasksPoolExtensions
{
    /// <summary>
    /// Dispatch a command in a background task
    /// </summary>
    /// <param name="this"></param>
    /// <param name="name"></param>
    /// <param name="preDispatch"></param>
    /// <param name="dispatcher"></param>
    /// <param name="command"></param>
    /// <param name="cleanMemory"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static int Run(this TasksPool @this, string name, ICommandDispatcher dispatcher, ICommand command, Func<ValueTask>? preDispatch, bool cleanMemory, ILogger logger)
    {
        return @this.Run(
            name,
            async () =>
            {
                logger.TaskStarted(name);

                if (preDispatch is not null)
                    await preDispatch();
                var result = await dispatcher.DispatchDynamicAsync(command);

                logger.TaskEnded(name, result);
            },
            cleanMemory
        );
    }
    /// <summary>
    /// Dispatch a command in a background task
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="this"></param>
    /// <param name="name"></param>
    /// <param name="dispatcher"></param>
    /// <param name="command"></param>
    /// <param name="preDispatch"></param>
    /// <param name="cleanMemory"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static int Run<TResponse>(this TasksPool @this, string name, ICommandDispatcher dispatcher, ICommand<TResponse> command, Func<ValueTask>? preDispatch, bool cleanMemory, ILogger logger)
    {
        return @this.Run(
            name,
            async () =>
            {
                logger.TaskStarted(name);

                if (preDispatch is not null)
                    await preDispatch();
                var result = await dispatcher.DispatchAsync(command);

                logger.TaskEnded(name, result);
            },
            cleanMemory
        );
    }
}

internal static partial class TasksPoolExtensionsLoggerExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Start task {Name}")]
    public static partial void TaskStarted(this ILogger logger, string name);
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "End task {Name} with result {Result}")]
    public static partial void TaskEnded(this ILogger logger, string name, object? result);
}
