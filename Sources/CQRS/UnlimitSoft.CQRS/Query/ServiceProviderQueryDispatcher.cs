using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Mediator;

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
    /// <param name="errorText">Default text used to response in Inotify object when validation not success.</param>
    /// <param name="errorTransforms">Conver error to a Dictionary where key is a propertyName with an error and value is all error description.</param>
    public ServiceProviderQueryDispatcher(
        IServiceProvider provider,
        bool validate = true,
        string? errorText = null,
        Func<IEnumerable<ValidationFailure>, IDictionary<string, string[]>>? errorTransforms = null
    )
    {
        var logger = provider.GetService<ILogger<ServiceProviderMediator>>();
        _mediator = new ServiceProviderMediator(provider, validate, false, errorText, errorTransforms, logger);
    }

    /// <inheritdoc />
    public ValueTask<Result<TResponse>> DispatchAsync<TResponse>(IServiceProvider provider, IQuery<TResponse> query, CancellationToken ct = default) => _mediator.SendAsync(provider, query, ct);
}