using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IRequestHandlerPipeline
{
    /// <summary>
    /// Indicates the order of execution of the pipeline. All extra handlers are executed in order before and after the main handler
    /// </summary>
    int Order { get; }
}
/// <summary>
/// Allow add diferent operation after execution of the handler
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IRequestHandlerPipeline<in TRequest, TResponse> : IRequestHandlerPipeline
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Operation to execute before the handler this is executed even before the init handler interface
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask PreHandleAsync(TRequest command, CancellationToken ct);
    /// <summary>
    /// Operation to execute after the handler this is executed even after the end handler interface
    /// </summary>
    /// <param name="command"></param>
    /// <param name="response"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<TResponse> PostHandleAsync(TRequest command, TResponse response, CancellationToken ct);
}