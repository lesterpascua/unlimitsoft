using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send a request and match this request with a handler request using the service root provider. If in the configuration is specified useScope a new scope will be used for the request.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
    /// <summary>
    /// Send a request and match this request with a handler request using the service provider in the argument. 
    /// </summary>
    /// <remarks>Use this to execute in the same scope of the executor.</remarks>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="provider"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SendAsync<TResponse>(IServiceProvider provider, IRequest<TResponse> request, CancellationToken ct = default);

    /// <summary>
    /// Send a request and match this request with a handler request using the service provider in the argument. 
    /// </summary>
    /// <remarks>Use this to execute in the same scope of the executor.</remarks>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SafeSendAsync<TResponse>(IRequest<Result<TResponse>> request, CancellationToken ct = default);
    /// <summary>
    /// Send a request and match this request with a handler request using the service provider in the argument. 
    /// </summary>
    /// <remarks>Use this to execute in the same scope of the executor.</remarks>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="provider"></param>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<TResponse>> SafeSendAsync<TResponse>(IServiceProvider provider, IRequest<Result<TResponse>> request, CancellationToken ct = default);
}