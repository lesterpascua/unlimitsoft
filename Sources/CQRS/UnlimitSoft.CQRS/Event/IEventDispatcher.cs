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
}
