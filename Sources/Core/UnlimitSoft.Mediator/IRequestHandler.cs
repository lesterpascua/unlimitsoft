using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public interface IRequestHandler { }
/// <summary>
/// 
/// </summary>
/// <typeparam name="TRequest">Type of the request where this handler is associate.</typeparam>
/// <typeparam name="TResponse">Type of the response of the command</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Define all logic asociate to the implemented <paramref name="request"/>.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
}