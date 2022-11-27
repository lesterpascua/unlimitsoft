using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// 
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Send a command to his command handler. This operation must execute in new scope.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> DispatchAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);
    /// <summary>
    /// Send command to his handler using specific service provider. This operation use same scope of provider.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> DispatchAsync<TResponse>(IServiceProvider provider, ICommand<TResponse> command, CancellationToken ct = default);
}
/// <summary>
/// Extension method for Command dispatcher
/// </summary>
public static class ICommandDispatcherExtensions
{
    /// <summary>
    /// Send a command to his command handler. This operation must execute in new scope.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<IResponse> DispatchAsync(this ICommandDispatcher @this, ICommand command, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Send a command to his command handler. This operation must execute in new scope.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="provider"></param>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<IResponse> DispatchAsync(this ICommandDispatcher @this, IServiceProvider provider, ICommand command, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}