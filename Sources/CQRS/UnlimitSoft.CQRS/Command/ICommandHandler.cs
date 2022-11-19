using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Interfaz for all class for handler command
/// </summary>
public interface ICommandHandler
{
}
/// <summary>
/// Handle command of generic type.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<TCommand> : ICommandHandler where TCommand : ICommand
{
    /// <summary>  
    /// Handler a command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<ICommandResponse> HandleAsync(TCommand command, CancellationToken ct = default);
}
