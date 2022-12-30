using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// 
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="provider"></param>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> DispatchAsync<TResponse>(IServiceProvider provider, IQuery<TResponse> query, CancellationToken ct = default);
}
/// <summary>
/// Extension method for Command dispatcher
/// </summary>
public static class IQueryDispatcherExtensions
{
    /// <summary>
    /// Send a command to his command handler. This operation must execute in new scope.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="provider"></param>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static ValueTask<IResponse> DispatchAsync(this IQueryDispatcher @this, IServiceProvider provider, IQuery query, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}