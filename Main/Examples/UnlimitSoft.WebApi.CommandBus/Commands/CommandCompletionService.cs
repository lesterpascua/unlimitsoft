using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;

namespace UnlimitSoft.WebApi.CommandBus.Commands;


/// <summary>
/// 
/// </summary>
public sealed class CommandCompletionService : ICommandCompletionService
{
    private readonly ICommandBus _bus;

    public CommandCompletionService(ICommandBus bus)
    {
        _bus = bus;
    }

    public async Task<Message.IResult> CompleteAsync(ICommand command, Message.IResult response, Exception? error = null, CancellationToken ct = default)
    {
        if (!response.IsSuccess && command is ISchedulerCommand scheduler)
        {
            scheduler.SetDelay(TimeSpanUtility.DuplicateRetryTime(scheduler.GetDelay(), TimeSpan.FromSeconds(30)));
            await _bus.SendAsync(command, ct);
        }
        return response;
    }
}
