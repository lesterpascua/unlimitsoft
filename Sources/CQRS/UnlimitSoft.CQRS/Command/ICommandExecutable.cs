using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Web.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandExecutable<TResponse> : ICommand
{
}
/// <summary>
/// 
/// </summary>
public static class ICommandExecutableExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="command"></param>
    /// <param name="dispatcher"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<(ICommandResponse, TResponse?)> ExecuteAsync<TResponse>(this ICommandExecutable<TResponse> command, ICommandDispatcher dispatcher, CancellationToken ct = default)
    {
        var response = await dispatcher.DispatchAsync(command, ct);
        if (response.IsSuccess)
            return (response, response.GetBody<TResponse>());

        return (response, default);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="command"></param>
    /// <param name="provider"></param>
    /// <param name="dispatcher"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<(ICommandResponse, TResponse?)> ExecuteAsync<TResponse>(this ICommandExecutable<TResponse> command, IServiceProvider provider, ICommandDispatcher dispatcher, CancellationToken ct = default)
    {
        var response = await dispatcher.DispatchAsync(provider, command, ct);
        if (response.IsSuccess)
            return (response, response.GetBody<TResponse>());

        return (response, default);
    }
}
