using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// Default job processor.
/// </summary>
public class DefaultJobProcessor : IJobProcessor
{
    private readonly IServiceProvider _provider;
    private readonly ICommandDispatcher _dispatcher;
    private readonly Func<Exception, Task>? _onError;
    private readonly ICommandCompletionService? _completionService;
    private readonly Func<IServiceProvider, ICommand, JobActivatorContext, Func<ICommand, CancellationToken, Task<ICommandResponse>>, CancellationToken, Task<ICommandResponse>>? _preeProcess;
    private readonly ILogger<DefaultJobProcessor>? _logger;

    private readonly string _errorCode;
    private readonly Dictionary<string, string[]> _errorBody;

    private static readonly JsonSerializerOptions _jsonSettings;

    /// <summary>
    /// Default error code raice if exist a problem in the process.
    /// </summary>
    public static string DefaultErrorCode = "-1";


    /// <summary>
    /// 
    /// </summary>
    static DefaultJobProcessor()
    {
        _jsonSettings = new()
        {
            WriteIndented = false,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="dispatcher"></param>
    /// <param name="errorCode"></param>
    /// <param name="onError"></param>
    /// <param name="preeProcess">Allow to invoke some action before the command processing.</param>
    /// <param name="completionService"> After finish some command processing will call the method <see cref="ICommandCompletionService.CompleteAsync(ICommand, ICommandResponse, Exception, CancellationToken)"/>
    /// </param>
    /// <param name="logger"></param>
    public DefaultJobProcessor(
        IServiceProvider provider,
        ICommandDispatcher dispatcher,
        string? errorCode = null,
        Func<Exception, Task>? onError = null,
        ICommandCompletionService? completionService = null,
        Func<IServiceProvider, ICommand, JobActivatorContext, Func<ICommand, CancellationToken, Task<ICommandResponse>>, CancellationToken, Task<ICommandResponse>>? preeProcess = null,
        ILogger<DefaultJobProcessor>? logger = null
    )
    {
        _logger = logger;
        _provider = provider;
        _dispatcher = dispatcher;
        _onError = onError;
        _preeProcess = preeProcess;
        _completionService = completionService;

        _errorCode = errorCode ?? DefaultErrorCode;
        _errorBody = new Dictionary<string, string[]> { [string.Empty] = new string[] { _errorCode } };

        Context = null!;        // This will be assigned in the creator parent.
    }

    /// <inheritdoc />
    public JobActivatorContext Context { get; set; }

    /// <inheritdoc />
    public async Task<ICommandResponse> ProcessAsync(string json, Type type)
    {
        var command = (ICommand)JsonSerializer.Deserialize(json, type, _jsonSettings)!;
        var props = command.GetProps<CommandProps>();
        if (props is ISchedulerCommandProps schedulerCommandProps) 
        {
            schedulerCommandProps.JobId = Context.BackgroundJob.Id;
            schedulerCommandProps.Retry = Context.GetJobParameter<int>(HangfireCommandBus.RetryParam);
            
            var delayTicks = Context.GetJobParameter<long?>(HangfireCommandBus.DelayTicksParam);
            if (delayTicks.HasValue)
                schedulerCommandProps.Delay = new TimeSpan(delayTicks.Value);
        }

        if (_preeProcess is null)
            return await RunAsync(command, CancellationToken);

        return await _preeProcess(_provider, command, Context, RunAsync, CancellationToken);
    }

    #region Private Methods
    /// <summary>
    /// Cancelation token of the operation
    /// </summary>
    private CancellationToken CancellationToken => Context?.CancellationToken.ShutdownToken ?? default;

    private async Task<ICommandResponse> RunAsync(ICommand command, CancellationToken ct)
    {
        Exception? err = null;
        ICommandResponse response;
        var props = command.GetProps<CommandProps>();

        var meta = Context.BackgroundJob;
        try
        {
            _logger?.LogDebug("Start process command: {@Command}", command);
            _logger?.LogInformation("Start process {Job} command: {Id}", meta.Id, props.Id);

            response = await _dispatcher.DispatchAsync(_provider, command, ct);
        }
        catch (Exception exc)
        {
            err = exc;
            _logger?.LogError(exc, "Error processing jobId: {JobId}, command: {@Command}", meta.Id, command);

            if (_onError != null)
                await _onError(exc);
            response = command.ErrorResponse(_errorBody);
        }

        if (_completionService is not null && (!props.Silent || !response.IsSuccess))
            response = await _completionService.CompleteAsync(command, response, err, CancellationToken);

        _logger?.LogDebug(@"End process
JobId: {JobId}
command: {@Command}
Response: {@Response}", meta.Id, command, response);
        _logger?.LogInformation("End process {Job} with error {Error}", meta.Id, err is not null || response?.IsSuccess == false);

        return response!;
    }
    #endregion
}
