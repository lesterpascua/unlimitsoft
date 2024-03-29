﻿using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// Indicate the command handler allow validate him self
/// </summary>
public interface IRequestHandlerValidator<TRequest> : IRequestHandler
    where TRequest : IRequest
{
    /// <summary>
    /// Build fluent validation rules.
    /// </summary>
    /// <param name="request">Request to validate</param>
    /// <param name="validator"></param>
    /// <param name="ct"></param>
    /// <returns>Allow return custome response before the validation process.</returns>
    ValueTask<IResponse> ValidatorAsync(TRequest request, RequestValidator<TRequest> validator, CancellationToken ct = default);
}