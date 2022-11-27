using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Interfaz for all class for handler command
/// </summary>
public interface ICommandHandler : IRequestHandler
{
}
/// <summary>
/// Command Handle
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandHandler<in TCommand, TResponse> : ICommandHandler, IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
