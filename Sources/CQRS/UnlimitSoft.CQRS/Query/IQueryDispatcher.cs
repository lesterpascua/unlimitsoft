using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// 
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Send query to his handler using specific service provider. This operation use same scope of provider.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="provider"></param>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> DispatchAsync<TResponse>(IServiceProvider provider, IQuery<TResponse> query, CancellationToken ct = default);
    /// <summary>
    /// Send query to his handler using specific service provider. This operation use same scope of provider.
    /// This method is optimize for command that return a result response
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SafeDispatchAsync<TResponse>(IServiceProvider provider, IQuery<Result<TResponse>> command, CancellationToken ct = default);
}
/// <summary>
/// Extension method for Command dispatcher
/// </summary>
public static class IQueryDispatcherExtensions
{
    /// <summary>
    /// Send a query to his command handler. This operation must execute in new scope.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="provider"></param>
    /// <param name="query"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async ValueTask<IResponse> DispatchDynamicAsync(this IQueryDispatcher @this, IServiceProvider provider, IQuery query, CancellationToken ct = default)
    {
        dynamic dynamicQuery = query;
        return await @this.DispatchAsync(provider, query: dynamicQuery, ct: ct);
    }
}