using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.CQRS.Command;


/// <summary>
/// Command that can be scheduler in the time.
/// </summary>
public interface ISchedulerCommand : ICommand
{
    /// <summary>
    /// Get jobId asociate with the command.
    /// </summary>
    public object GetJobId();
    /// <summary>
    /// Set the jobId asociate with the command.
    /// </summary>
    /// <param name="jobId"></param>
    public void SetJobId(object jobId);

    /// <summary>
    /// Get indicate how many time this command is retry
    /// </summary>
    public int? GetRetry();
    /// <summary>
    /// Set indicate how many time this command is retry
    /// </summary>
    /// <param name="retry"></param>
    public void SetRetry(int? retry);

    /// <summary>
    /// Get time to delay this command before procesed
    /// </summary>
    public TimeSpan? GetDelay();
    /// <summary>
    /// Set time to delay this command before procesed
    /// </summary>
    public void SetDelay(TimeSpan? dalay);
}
/// <summary>
/// 
/// </summary>
public static class ISchedulerCommandExtensions
{
    /// <summary>
    /// Allow retry a command when some error is happening.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="bus"></param>
    /// <param name="ex"></param>
    /// <param name="action">Execute action before reenqueue command.</param>
    /// <param name="maxDelay"></param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<object?> ErrorRetryAsync(this ISchedulerCommand @this, ICommandBus bus, Exception ex, Func<TimeSpan, Task>? action, TimeSpan? maxDelay = null, ILogger? logger = null, CancellationToken ct = default)
    {
        logger?.LogError(
            ex,
            "Error trying to execute operation {Type} using arguments: {@Command}",
            @this.GetType().FullName,
            @this
        );

        var delay = TimeSpanUtility.DuplicateRetryTime(@this.GetDelay(), maxDelay);
        @this.SetDelay(delay);

        if (action is not null)
            await action(delay);

        var jobId = await bus.SendAsync(@this, ct);
        logger?.LogInformation("Command will retry in {Delay} with id={JobId}", delay, jobId);

        return jobId;
    }
    /// <summary>
    /// Allow retry a command when some error is happening.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="bus"></param>
    /// <param name="ex"></param>
    /// <param name="maxDelay"></param>
    /// <param name="logger"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<object?> ErrorRetryAsync(this ISchedulerCommand @this, ICommandBus bus, Exception ex, TimeSpan? maxDelay = null, ILogger? logger = null, CancellationToken ct = default) => @this.ErrorRetryAsync(bus, ex, null, maxDelay, logger, ct);
}
