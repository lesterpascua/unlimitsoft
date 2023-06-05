using UnlimitSoft.CQRS.Command;

namespace UnlimitSoft.WebApi.CommandBus.Commands;


/// <summary>
/// 
/// </summary>
public sealed class TestCommand : Command<string, SchedulerCommandProps>, ISchedulerCommand
{
    public TestCommand()
    {
        Props = new SchedulerCommandProps();
    }

    public object? GetJobId() => Props!.JobId;
    public void SetJobId(object? jobId) => Props!.JobId = (string?)jobId;

    public int GetRetry() => Props!.Retry;
    public void SetRetry(int retry) => Props!.Retry = retry;

    public TimeSpan? GetDelay() => Props!.Delay;
    public void SetDelay(TimeSpan? delay) => Props!.Delay = delay;
}
/// <summary>
/// 
/// </summary>
public sealed class TestCommandHandler : ICommandHandler<TestCommand, string>, ICommandHandlerLifeCycle<TestCommand>
{
    /// <inheritdoc />
    public ValueTask InitAsync(TestCommand request, CancellationToken ct = default) => ValueTask.CompletedTask;
    /// <inheritdoc />
    public async ValueTask<string> HandleAsync(TestCommand request, CancellationToken ct = default)
    {
        await ValueTask.CompletedTask;

        throw new Exception("retry please");
    }
    /// <inheritdoc />
    public ValueTask EndAsync(TestCommand request, CancellationToken ct = default) => ValueTask.CompletedTask;
}
