using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IRequestHandler { }
/// <summary>
/// 
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}