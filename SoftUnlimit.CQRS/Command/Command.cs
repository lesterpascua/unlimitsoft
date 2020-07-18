using Newtonsoft.Json;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Base class for all command.
    /// </summary>
    public abstract class Command<T> : ICommand
        where T : CommandProps 
    {
        /// <summary>
        /// Get or set metadata props associate with the command.
        /// </summary>
        public T CommandProps { get; set; }

        /// <summary>
        /// Return metadata props associate with the command.
        /// </summary>
        /// <typeparam name="TProps"></typeparam>
        /// <returns></returns>
        TProps ICommand.GetProps<TProps>() => this.CommandProps as TProps;
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class SealedCommand : Command<CommandProps>
    {
        /// <summary>
        /// 
        /// </summary>
        public SealedCommand() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="props"></param>
        public SealedCommand(CommandProps props)
        {
            this.CommandProps = props;
        }
    }
}
