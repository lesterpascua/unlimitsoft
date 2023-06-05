using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Indicate the command handler allow livecycle him self
/// </summary>
public interface ICommandHandlerLifeCycle<TCommand> : ICommandHandler, IRequestHandlerLifeCycle<TCommand>
    where TCommand : ICommand
{
}