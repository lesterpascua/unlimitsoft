using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IMediator
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="provider"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct = default);
}