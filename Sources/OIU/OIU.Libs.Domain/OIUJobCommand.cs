using UnlimitSoft.CQRS.Command;

namespace OIU.Libs.Domain;


/// <summary>
/// 
/// </summary>
public class OIUJobCommandProps : OIUCommandProps, IProps
{
    /// <summary>
    /// If the command is a <see cref="ISchedulerCommand"/> indicate job identifier
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string? JobId { get; set; }
    /// <summary>
    /// If the command is a <see cref="ISchedulerCommand"/> indicate how many time the command was retry.
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public int Retry { get; set; }
    /// <summary>
    /// If the command is a <see cref="ISchedulerCommand"/> indicate the delay associate to the command
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public TimeSpan? Delay { get; set; }
}
/// <summary>
/// Command to represent change state operation
/// </summary>
public abstract class OIUJobCommand<TResponse> : Command<TResponse, OIUJobCommandProps>, ISchedulerCommand
{
    /// <summary>
    /// 
    /// </summary>
    protected OIUJobCommand() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="identityId"></param>
    /// <param name="correlationId"></param>
    protected OIUJobCommand(Guid identityId, string? correlationId)
    {
        Props = new OIUJobCommandProps
        {
            IdentityId = identityId,
            CorrelationId = correlationId,
        };
    }

    /// <summary>
    /// Get hangfire job asociate with the command
    /// </summary>
    /// <returns></returns>
    public object? GetJobId() => Props!.JobId;
    /// <inheritdoc />
    public void SetJobId(object? jobId) => Props!.JobId = (string?)jobId;

    /// <inheritdoc />
    public int GetRetry() => Props!.Retry;
    /// <inheritdoc />
    public void SetRetry(int retry) => Props!.Retry = retry;

    /// <inheritdoc />
    public TimeSpan? GetDelay() => Props!.Delay;
    /// <inheritdoc />
    public void SetDelay(TimeSpan? delay) => Props!.Delay = delay;
}