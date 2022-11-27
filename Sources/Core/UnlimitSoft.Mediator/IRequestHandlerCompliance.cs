using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// Indicate the command handler allow validate him self
/// </summary>
public interface IRequestHandlerCompliance<TRequest> : IRequestHandler
    where TRequest : IRequest
{
    /// <summary>
    /// Build fluent validation rules.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns>Allow return custome response before the validation process.</returns>
    ValueTask<IResponse> ComplianceV2Async(TRequest command, CancellationToken ct = default);
}