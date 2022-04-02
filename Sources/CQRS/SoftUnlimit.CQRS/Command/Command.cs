using System;

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
        public T Props { get; set; }

        /// <summary>
        /// Return metadata props associate with the command.
        /// </summary>
        /// <typeparam name="TProps"></typeparam>
        /// <returns></returns>
        TProps ICommand.GetProps<TProps>() => Props as TProps;
    }
}
