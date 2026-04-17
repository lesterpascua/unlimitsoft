using Microsoft.Extensions.Logging;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Message;

namespace OIU.Libs.Domain;


/// <summary>
/// Command to represent change state operation
/// </summary>
public sealed class OIUSchedulerCommandProps : CommandProps, IProps
{
    /// <inheritdoc />
    public Guid IdentityId { get; set; }
    /// <inheritdoc />
    public string? CorrelationId { get; set; }

    /// <summary>
    /// If the command is a <see cref="ISchedulerCommand"/> indicate job identifier
    /// </summary>
    internal string? JobId { get; set; }
    /// <summary>
    /// If the command is a <see cref="ISchedulerCommand"/> indicate how many time the command was retry.
    /// </summary>
    internal int Retry { get; set; }
    /// <summary>
    /// If the command is a <see cref="ISchedulerCommand"/> indicate the delay associate to the command
    /// </summary>
    internal TimeSpan? Delay { get; set; }
}
/// <summary>
/// Command to represent change state operation
/// </summary>
public abstract class OIUSchedulerCommand : Command<ScheduledResult, OIUSchedulerCommandProps>, ISchedulerCommand
{
    /// <summary>
    /// 
    /// </summary>
    protected OIUSchedulerCommand() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identityId"></param>
    /// <param name="correlationId"></param>
    protected OIUSchedulerCommand(Guid identityId, string? correlationId)
    {
        Props = new OIUSchedulerCommandProps
        {
            IdentityId = identityId,
            CorrelationId = correlationId,
        };
    }

    /// <summary>
    /// Get hangfire job asociate with the command
    /// </summary>
    /// <returns></returns>
    public object? GetJobId() => Props.JobId;
    /// <inheritdoc />
    public void SetJobId(object? jobId) => Props.JobId = (string?)jobId;

    /// <inheritdoc />
    public int GetRetry() => Props.Retry;
    /// <inheritdoc />
    public void SetRetry(int retry) => Props.Retry = retry;

    /// <inheritdoc />
    public TimeSpan? GetDelay() => Props.Delay;
    /// <inheritdoc />
    public void SetDelay(TimeSpan? delay) => Props.Delay = delay;
}

/// <summary>
/// 
/// </summary>
public static class OIUSchedulerCommandExtensions
{
    /// <summary>
    /// Try to run a command and if exist error will retry the operation 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="commandBus"></param>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    /// <param name="getMaxDelay">Maximun amount of time the job will be rescheduler</param>
    /// <param name="beforeSentInTheBusAction">Operation to perform before go to the command bus</param>
    /// <param name="onError">Process error with custom logic. If return null will execute the default logic, to stop return some response.</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<IResult> RunWithRetry(
        this ISchedulerCommand command,
        ICommandBus commandBus,
        Func<ICommand, CancellationToken, ValueTask<IResult>> next,
        Func<ICommand, TimeSpan?>? getMaxDelay = null,
        Func<TimeSpan, Task>? beforeSentInTheBusAction = null,
        Func<ISchedulerCommand, Exception, ScheduledResult>? onError = null,
        ILogger? logger = null,
        CancellationToken ct = default
    )
    {
        try
        {
            var result = await next(command, ct);
            if ((ScheduledResult?)result.GetValue() == ScheduledResult.Reenqueue)
                await RetryAsync(command, commandBus, getMaxDelay, beforeSentInTheBusAction, logger, null, ct);

            return result;
        }
        catch (Exception ex)
        {
            if (onError is not null)
            {
                var result = onError(command, ex);
                return Result.Ok(result);
            }

            await RetryAsync(command, commandBus, getMaxDelay, beforeSentInTheBusAction, logger, ex, ct);
            return Result.Ok(ScheduledResult.Error);
        }

        // ========================================================================================================================
        static async Task RetryAsync(ISchedulerCommand command, ICommandBus commandBus, Func<ICommand, TimeSpan?>? getMaxDelay, Func<TimeSpan, Task>? beforeSentInTheBusAction, ILogger? logger, Exception? ex, CancellationToken ct)
        {
            var delay = getMaxDelay?.Invoke(command);
            await command.ErrorRetryAsync(commandBus, ex, beforeSentInTheBusAction, delay, logger, ct);
        }
    }
}