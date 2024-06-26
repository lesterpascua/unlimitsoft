﻿using Microsoft.Extensions.Logging;
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
    /// <param name="dispatcher"></param>
    /// <param name="command"></param>
    /// <param name="cleanMemory"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static int Run(this TasksPool @this, string name, ICommandDispatcher dispatcher, ICommand command, bool cleanMemory, ILogger logger)
    {
        return @this.Run(
            name,
            async () =>
            {
                logger.LogInformation("Start task {Name}", name);
                var result = await dispatcher.DispatchDynamicAsync(command);

                logger.LogInformation("End task {Name} with result {Result}", name, result);
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
    /// <param name="cleanMemory"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static int Run<TResponse>(this TasksPool @this, string name, ICommandDispatcher dispatcher, ICommand<TResponse> command, bool cleanMemory, ILogger logger)
    {
        return @this.Run(
            name,
            async () =>
            {
                logger.LogInformation("Start task {Name}", name);
                var result = await dispatcher.DispatchAsync(command);

                logger.LogInformation("End task {Name} with result {Result}", name, result);
            },
            cleanMemory
        );
    }
}
