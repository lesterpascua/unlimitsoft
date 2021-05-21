using FluentValidation;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command.Validation
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete("ICommandHandlerValidator")]
    public interface ICommandSharedCache
    {
        /// <summary>
        /// 
        /// </summary>
        object Cache { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICommand Get();
    }
    /// <summary>
    /// Wrap command into object to allow capability to share cache between all command processor flow. Validation, Compliance, Handling.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    [Obsolete("ICommandHandlerValidator")]
    public class CommandSharedCache<TCommand> : ICommandSharedCache
        where TCommand : ICommand
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public CommandSharedCache(TCommand command)
        {
            this.Command = command;
        }

        /// <summary>
        /// 
        /// </summary>
        public TCommand Command { get; }
        /// <summary>
        /// 
        /// </summary>
        public object Cache { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ICommand Get() => this.Command;
    }
}
