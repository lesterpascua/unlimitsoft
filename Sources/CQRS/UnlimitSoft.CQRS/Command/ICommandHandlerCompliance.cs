using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Indicate the command handler allow compliance him self
/// </summary>
public interface ICommandHandlerCompliance<TCommand> : ICommandHandler, IRequestHandlerCompliance<TCommand>
    where TCommand : ICommand
{
}
