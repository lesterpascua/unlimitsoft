using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IRequestHandlerLifeCycle<TRequest> : IRequestHandler
    where TRequest : IRequest
{
    /// <summary>
    /// This will be executed every time the request is executed just at the beginning
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask InitAsync(TRequest request, CancellationToken ct = default);
    /// <summary>
    /// This will be executed every time the request is executed just at the end
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask EndAsync(TRequest request, CancellationToken ct = default);
}
