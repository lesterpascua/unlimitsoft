using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Mediator.Pipeline;


/// <summary>
/// 
/// </summary>
public interface IRequestHandlerPostPipeline { }
/// <summary>
/// 
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TPipeline"></typeparam>
public interface IRequestHandlerPostPipeline<in TRequest, in THandler, TResponse, TPipeline> : IRequestHandlerPostPipeline
    where TRequest : IRequest<TResponse>
    where THandler : IRequestHandler
    where TPipeline : IRequestHandlerPostPipeline<TRequest, THandler, TResponse, TPipeline>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="handler"></param>
    /// <param name="response"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task HandleV2Async(TRequest command, THandler handler, TResponse response, CancellationToken ct);
}