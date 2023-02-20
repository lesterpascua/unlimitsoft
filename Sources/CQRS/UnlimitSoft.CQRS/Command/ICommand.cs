using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Interface to define a Command 
/// </summary>
public interface ICommand : IRequest
{
    /// <summary>
    /// Get command name
    /// </summary>
    /// <returns></returns>
    string GetName();
    /// <summary>
    /// Return metadata props associate with the command.
    /// </summary>
    /// <returns></returns>
    CommandProps GetProps();

    /// <summary>
    /// Set metadata props associate with the command.
    /// </summary>
    /// <param name="props"></param>
    /// <returns>Previous value</returns>
    void SetProps(CommandProps props);
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ICommand<out TResponse> : ICommand, IRequest<TResponse>
{
}