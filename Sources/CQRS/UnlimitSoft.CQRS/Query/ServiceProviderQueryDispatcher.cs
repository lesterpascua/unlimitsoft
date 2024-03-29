﻿using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Query provider dispatcher using and standard IServiceProvider to locate the QueryHandler associate with a query.
/// </summary>
public sealed class ServiceProviderQueryDispatcher : IQueryDispatcher
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="validate">Enable command validation after execute associate handler.</param>
    /// <param name="errorTransforms">Conver error to a Dictionary where key is a propertyName with an error and value is all error description.</param>
    public ServiceProviderQueryDispatcher(
        IServiceProvider provider,
        bool validate = true,
        Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>>? errorTransforms = null
    )
    {
        var logger = provider.GetService<ILogger<ServiceProviderMediator>>();
        _mediator = new ServiceProviderMediator(provider, validate, false, errorTransforms, logger);
    }

    /// <inheritdoc />
    public ValueTask<Result<TResponse>> DispatchAsync<TResponse>(IServiceProvider provider, IQuery<TResponse> query, CancellationToken ct = default) => _mediator.SendAsync(provider, query, ct);
    /// <inheritdoc />
    public ValueTask<Result<TResponse>> SafeDispatchAsync<TResponse>(IServiceProvider provider, IQuery<Result<TResponse>> query, CancellationToken ct = default) => _mediator.SafeSendAsync(provider, query, ct);
}