using FluentValidation;
using SoftUnlimit.CQRS.Command.Validation;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Indicate the command handler allow validate him self
    /// </summary>
    public interface ICommandHandlerValidator<TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        /// <summary>
        /// Build fluen validation rules.
        /// </summary>
        /// <param name="validator"></param>
        IValidator BuildValidator(CommandValidator<TCommand> validator);
    }
}
