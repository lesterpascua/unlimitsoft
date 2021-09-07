﻿using SoftUnlimit.CQRS.Message;
using System;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command.Compliance
{
    /// <summary>
    /// This interface is only for reference please inplement ICommandCompliance in TCommand to correct resolution using DPI
    /// </summary>
    [Obsolete("Use ICommandHandlerCompliance")]
    public interface ICommandCompliance
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sharedCache"></param>
        /// <returns></returns>
        Task<CommandResponse> ExecuteAsync(ICommand command, object sharedCache);
    }
    /// <summary>
    /// 
    /// </summary>
    [Obsolete("Use ICommandHandlerCompliance")]
    public interface ICommandCompliance<TCommand> : ICommandCompliance
        where TCommand : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="sharedCache"></param>
        /// <returns></returns>
        Task<CommandResponse> ExecuteAsync(TCommand command, object sharedCache);
    }
}
