using UnlimitSoft.CQRS.Message;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command.Pipeline;

/// <summary>
/// 
/// </summary>
public interface ICommandHandlerPostPipeline { }
/// <summary>
/// 
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <typeparam name="TPipeline"></typeparam>
public interface ICommandHandlerPostPipeline<in TCommand, in THandler, TPipeline> : ICommandHandlerPostPipeline
    where TCommand : ICommand
    where THandler : ICommandHandler
    where TPipeline : ICommandHandlerPostPipeline<TCommand, THandler, TPipeline>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="handler"></param>
    /// <param name="response"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public Task HandleAsync(TCommand command, THandler handler, ICommandResponse response, CancellationToken ct);
}