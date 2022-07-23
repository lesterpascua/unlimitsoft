using UnlimitSoft.CQRS.Command.Validation;
using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command;

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
    /// <returns>Allow return custome response before the validation process.</returns>
    ValueTask<ICommandResponse> ValidatorAsync(TCommand command, CommandValidator<TCommand> validator, CancellationToken ct = default);
}