using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;

/// <summary>
/// Indicate the command handler allow validate him self
/// </summary>
public interface ICommandHandlerValidator<TCommand> : ICommandHandler, IRequestHandlerValidator<TCommand>
    where TCommand : ICommand
{
}