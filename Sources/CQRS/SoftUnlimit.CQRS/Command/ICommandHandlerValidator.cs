using SoftUnlimit.CQRS.Command.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.CQRS.Command
{
    /// <summary>
    /// Indicate the command handler allow validate him self
    /// </summary>
    public interface ICommandHandlerValidator<TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        /// <summary>
        /// Build fluent validation rules.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="validator"></param>
        /// <param name="ct"></param>
        ValueTask ValidatorAsync(TCommand command, CommandValidator<TCommand> validator, CancellationToken ct = default);
    }
}
