using System;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Web.Security;

namespace UnlimitSoft.WebApi.Sources.CQRS.Command;


public class AsyncTestCommand : MySchedulerCommand
{
    /// <summary>
    /// Deserialization constructor.
    /// </summary>
    [System.Text.Json.Serialization.JsonConstructor]
    public AsyncTestCommand() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="user"></param>
    public AsyncTestCommand(Guid id, IdentityInfo? user = null) :
        base(id, user)
    {
    }

    /// <summary>
    /// Command identifier.
    /// </summary>
    public Guid Id { get; set; }
}
public class AsyncTestCommandHandler : IMyCommandHandler<AsyncTestCommand>
{
    private readonly ICommandBus _commandBus;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandBus"></param>
    public AsyncTestCommandHandler(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<ICommandResponse> HandleAsync(AsyncTestCommand command, CancellationToken ct = default)
    {
        command.Props.Delay = TimeSpanUtility.DuplicateRetryTime(command.Props.Delay);
        await _commandBus.SendAsync(command, ct);


        return command.OkResponse(body: $"AsyncTestCommand: {command.Props.JobId} => ok");
    }
}
