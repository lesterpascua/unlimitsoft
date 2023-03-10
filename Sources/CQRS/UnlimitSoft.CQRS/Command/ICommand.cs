using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Interface to define a Command 
/// </summary>
public interface ICommand : IRequest
{
    /// <summary>
    /// Return metadata props associate with the command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? GetProps<T>() where T : CommandProps;
    /// <summary>
    /// Set metadata props associate with the command.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="props"></param>
    /// <returns>Previous value</returns>
    void SetProps<T>(T? props) where T : CommandProps;
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ICommand<out TResponse> : ICommand, IRequest<TResponse>
{
}