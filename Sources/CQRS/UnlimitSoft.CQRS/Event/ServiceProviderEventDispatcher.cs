using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Event;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public class ServiceProviderEventDispatcher : IEventDispatcher
{
    private readonly ServiceProviderMediator _mediator;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="useScope"></param>
    public ServiceProviderEventDispatcher(
        IServiceProvider provider,
        bool useScope = true
    )
    {
        var logger = provider.GetService<ILogger<ServiceProviderMediator>>();
        _mediator = new ServiceProviderMediator(provider, false, useScope, logger: logger);
    }

    /// <inheritdoc />
    public ValueTask<Result<IResponse>> DispatchAsync(IEvent @event, CancellationToken ct = default) => _mediator.SendAsync(@event, ct);
    /// <inheritdoc />
    public ValueTask<Result<IResponse>> DispatchAsync(IServiceProvider provider, IEvent @event, CancellationToken ct = default) => _mediator.SendAsync(provider, @event, ct);
}
