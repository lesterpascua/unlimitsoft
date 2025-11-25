using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// 
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatch event to asociate with the register event handler.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<IResponse>> DispatchAsync(IEvent @event, CancellationToken ct = default);
    /// <summary>
    ///  Dispatch event to asociate with the register event handler in the same scope of the parent.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<Result<IResponse>> DispatchAsync(IServiceProvider provider, IEvent @event, CancellationToken ct = default);

    /// <summary>
    /// Dispatch event to asociate with multiples register event handlers.
    /// </summary>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<AgregateResult<IResponse>> PublishAsync(IEvent @event, CancellationToken ct = default);
    /// <summary>
    /// Dispatch event to asociate with multiples register event handlers in the same scope of the parent.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="event"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    ValueTask<AgregateResult<IResponse>> PublishAsync(IServiceProvider provider, IEvent @event, CancellationToken ct = default);
}
