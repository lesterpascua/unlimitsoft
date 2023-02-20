﻿using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
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
    private readonly bool _incIfRetryDetect;
    private readonly IBackgroundJobClient _client;
    private readonly Func<ICommand, Task>? _preeSend;
    private readonly ILogger<HangfireCommandBus>? _logger;

    private static readonly JsonSerializerOptions _serializeJsonSettings;

    /// <summary>
    /// Name of the parameter used for props
    /// </summary>
    public const string PropsParam = "_Props";

    /// <summary>
    /// 
    /// </summary>
    static HangfireCommandBus()
    {
        _serializeJsonSettings = new()
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
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
        if (_preeSend is not null)
            await _preeSend(command);

        var jobId = CreateJob(command, type);
        if (jobId is not null)
            _logger?.LogDebug("Create background job with Id: {JobId}", jobId);
        return jobId;
    }

    #region Private Methods
    private string? CreateJob(ICommand command, Type type)
    {
        string? jobId;
        TimeSpan? delay;
        var props = command.GetProps();
        var connection = JobStorage.Current.GetConnection();

        var scheduler = command as ISchedulerCommand;
        if (_incIfRetryDetect && scheduler is not null)
            scheduler.SetRetry(scheduler.GetRetry() + 1);

        // If not scheduler command go and enqueue a new command
        if (scheduler is null || (delay = scheduler.GetDelay()) is null || delay == TimeSpan.Zero)
        {
            var json = SerializeWithoutProps(command, props);
            jobId = _client.Enqueue<IJobProcessor>(processor => processor.ProcessAsync(json, type));
            return UpdateJobParameters(connection, jobId, props);
        }

        jobId = (string?)scheduler.GetJobId();
        // If the jobid is null we can't track the command history them create a new scheduler.
        if (jobId is null)
        {
            var json = SerializeWithoutProps(command, props);
            jobId = _client.Schedule<IJobProcessor>(processor => processor.ProcessAsync(json, type), delay.Value);
            return UpdateJobParameters(connection, jobId, props);
        }

        // Otherwise reenqueue the same command
        jobId = UpdateJobParameters(connection, jobId, props);
        var state = new ScheduledState(delay.Value) { Reason = $"Delay the command {delay.Value}" };
        _client.ChangeState(jobId, state);

        return jobId;
    }
    /// <summary>
    /// Serialize the command witout the props.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="props"></param>
    /// <returns></returns>
    private static string SerializeWithoutProps(ICommand command, CommandProps? props)
    {
        command.SetProps(null);
        var json = JsonSerializer.Serialize<object>(command, _serializeJsonSettings);
        command.SetProps(props);
        return json;
    }
    /// <summary>
    /// Update the job parameters to trace retry and delay argumets
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="jobId"></param>
    /// <param name="props"></param>
    /// <returns></returns>
    private static string UpdateJobParameters(IStorageConnection connection, string jobId, CommandProps? props)
    {
        if (props is null)
            return jobId;
        
        var json = JsonSerializer.Serialize<object>(props, _serializeJsonSettings);
        connection.SetJobParameter(jobId, PropsParam, json);
        return jobId;
    }
    #endregion
}
