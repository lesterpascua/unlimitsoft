using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Implement a command dispatcher resolving all handler by a ServiceProvider.
/// </summary>
public sealed class ServiceProviderCommandDispatcher : ICommandDispatcher
{
    private readonly ServiceProviderMediator _mediator;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="validate">Enable command validation after execute associate handler.</param>
    /// <param name="useScope">Create scope to resolve element in DPI.</param>
    /// <param name="errorTransforms">Conver error to a Dictionary where key is a propertyName with an error and value is all error description.</param>
    public ServiceProviderCommandDispatcher(
        IServiceProvider provider, 
        bool validate = true,
        bool useScope = true,
        Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>>? errorTransforms = null
    )
    {
        var logger = provider.GetService<ILogger<ServiceProviderMediator>>();
        _mediator = new ServiceProviderMediator(provider, validate, useScope, errorTransforms, logger);
    }

    /// <inheritdoc />
    public ValueTask<Result<TResponse>> DispatchAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default) => _mediator.SendAsync(command, ct);
    /// <inheritdoc />
    public ValueTask<Result<TResponse>> DispatchAsync<TResponse>(IServiceProvider provider, ICommand<TResponse> command, CancellationToken ct = default) => _mediator.SendAsync(provider, command, ct);
}