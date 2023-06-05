using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Message;


/// <summary>
/// Allow perform operation asociate with some complete command operation.
/// </summary>
public interface ICommandCompletionService
{
    /// <summary>
    /// Send a new notification throw a bus.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="response"></param>
    /// <param name="error">If some exception happened send here</param>
    /// <param name="ct"></param>
    /// <returns>Response after pass for completion service. Normally is the same supplied as argument.</returns>
    Task<IResult> CompleteAsync(ICommand command, IResult response, Exception? error = null, CancellationToken ct = default);
}
