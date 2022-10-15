using Hangfire;
using Hangfire.States;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// 
/// </summary>
public class HangfireCommandBus : ICommandBus
{
    private readonly IBackgroundJobClient _client;
    private readonly Func<ICommand, Task>? _preeSend;
    private readonly bool _incIfRetryDetect;
    private readonly ILogger<HangfireCommandBus>? _logger;

    private static readonly JsonSerializerOptions _jsonSettings;
    private const string Reason = "Scheduled in the same jobId to keep history";

    /// <summary>
    /// Name of the parameter used for retry
    /// </summary>
    public const string RetryParam = "_RetryCount";
    /// <summary>
    /// Name of the paramer used for delay the command
    /// </summary>
    public const string DelayTicksParam = "_DelayTicks";


    /// <summary>
    /// 
    /// </summary>
    static HangfireCommandBus()
    {
        _jsonSettings = new()
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="preeSendCommand">Before enqueue the command execute this function.</param>
    /// <param name="incIfRetryDetect">Indicate increment the retry command counter if detect a retry.</param>
    /// <param name="logger"></param>
    public HangfireCommandBus(
        IBackgroundJobClient client,
        Func<ICommand, Task>? preeSendCommand = null,
        bool incIfRetryDetect = true,
        ILogger<HangfireCommandBus>? logger = null
    )
    {
        _client = client;
        _preeSend = preeSendCommand;
        _incIfRetryDetect = incIfRetryDetect;
        _logger = logger;
    }


    /// <inheritdoc />
    public void Dispose() => GC.SuppressFinalize(this);

    /// <inheritdoc />
    public async Task<object?> SendAsync(ICommand command, CancellationToken ct)
    {
        var type = command.GetType();
        var json = JsonSerializer.Serialize<object>(command, _jsonSettings);
        if (_preeSend is not null)
            await _preeSend(command);

        var jobId = CreateJob(command, type, json);
        if (jobId is not null)
            _logger?.LogDebug("Create background job with Id: {JobId}", jobId);
        return jobId;
    }

    #region Private Methods
    private string? CreateJob(ICommand command, Type type, string json)
    {
        string? jobId;
        var props = command.GetProps<CommandProps>();

        // If not scheduler command go and enqueue a new command
        if (props is not ISchedulerCommandProps schedulerCommandProps || !schedulerCommandProps.Delay.HasValue || schedulerCommandProps.Delay == TimeSpan.Zero)
            return _client.Enqueue<IJobProcessor>(processor => processor.ProcessAsync(json, type));

        if (_incIfRetryDetect)
            schedulerCommandProps.Retry = schedulerCommandProps.Retry.HasValue ? schedulerCommandProps.Retry + 1 : 0;

        // If the jobid is null we can't track the command history them create a new scheduler.
        if (schedulerCommandProps.JobId is null)
        {
            jobId = _client.Schedule<IJobProcessor>(processor => processor.ProcessAsync(json, type), schedulerCommandProps.Delay.Value);
            return UpdateJobParameters(jobId, schedulerCommandProps.Retry ?? 0, schedulerCommandProps.Delay.Value.Ticks);
        }

        // Otherwise reenqueue the same command
        jobId = UpdateJobParameters(
            schedulerCommandProps.JobId!.ToString(),
            schedulerCommandProps.Retry ?? 0,
            schedulerCommandProps.Delay.Value.Ticks
        );
        var state = new ScheduledState(schedulerCommandProps.Delay.Value) { Reason = Reason };
        _client.ChangeState(jobId, state);

        return jobId;
    }
    /// <summary>
    /// Update the job parameters to trace retry and delay argumets
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="retry"></param>
    /// <param name="delayTicks"></param>
    /// <returns></returns>
    private static string UpdateJobParameters(string jobId, int retry, long delayTicks)
    {
        var connection = JobStorage.Current.GetConnection();
        connection.SetJobParameter(jobId, RetryParam, retry.ToString());
        connection.SetJobParameter(jobId, DelayTicksParam, delayTicks.ToString());
        return jobId;
    }
    #endregion
}
