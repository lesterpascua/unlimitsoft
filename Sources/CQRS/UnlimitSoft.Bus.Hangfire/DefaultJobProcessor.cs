using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.Bus.Hangfire;


/// <summary>
/// Default job processor.
/// </summary>
public sealed class DefaultJobProcessor<TProps> : IJobProcessor
    where TProps : CommandProps
{
    private readonly IServiceProvider _provider;
    private readonly ICommandDispatcher _dispatcher;
    private readonly Func<ICommand, Exception, Task>? _onError;
    private readonly ICommandCompletionService? _completionService;
    private readonly ProcessCommandMiddleware? _middleware;
    private readonly ILogger<DefaultJobProcessor<TProps>>? _logger;

    private readonly string _errorCode;
    private readonly Dictionary<string, string[]> _errorBody;

    private static readonly JsonSerializerOptions _deserializerJsonSettings;

    /// <summary>
    /// Default error code raice if exist a problem in the process.
    /// </summary>
    public const string DefaultErrorCode = "-1";


    /// <summary>
    /// 
    /// </summary>
    static DefaultJobProcessor()
    {
        _deserializerJsonSettings = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="dispatcher"></param>
    /// <param name="errorCode"></param>
    /// <param name="onError"></param>
    /// <param name="middleware">Allow to invoke some action before the command processing.</param>
    /// <param name="completionService"> After finish some command processing will call the method <see cref="ICommandCompletionService.CompleteAsync(ICommand, IResponse, Exception?, CancellationToken)"/>
    /// </param>
    /// <param name="logger"></param>
    public DefaultJobProcessor(
        IServiceProvider provider,
        ICommandDispatcher dispatcher,
        string? errorCode = null,
        Func<ICommand, Exception, Task>? onError = null,
        ICommandCompletionService? completionService = null,
        ProcessCommandMiddleware? middleware = null,
        ILogger<DefaultJobProcessor<TProps>>? logger = null
    )
    {
        _logger = logger;
        _provider = provider;
        _dispatcher = dispatcher;
        _onError = onError;
        _middleware = middleware;
        _completionService = completionService;

        _errorCode = errorCode ?? DefaultErrorCode;
        _errorBody = new Dictionary<string, string[]> { [string.Empty] = new string[] { _errorCode } };

        Context = null!;        // This will be assigned in the creator parent.
    }

    /// <inheritdoc />
    public JobActivatorContext Context { get; set; }

    /// <inheritdoc />
    public async Task<IResult> ProcessAsync(string json, Type type)
    {
        var command = (ICommand)JsonSerializer.Deserialize(json, type, _deserializerJsonSettings)!;
        var props = Context.GetJobParameter<JobParams>(HangfireCommandBus.PropsParam);

        // Only assign if props is not null and is scheduler command.
        if (command is ISchedulerCommand scheduler)
        {
            scheduler.SetRetry(props.Retry);
            scheduler.SetDelay(props.Delay);
            scheduler.SetJobId(Context.BackgroundJob.Id);
        }

        if (_middleware is null)
            return await RunAsync(command, CancellationToken);

        return await _middleware(_provider, command, Context, RunAsync, CancellationToken);
    }

    #region Private Methods
    /// <summary>
    /// Cancelation token of the operation
    /// </summary>
    private CancellationToken CancellationToken => Context?.CancellationToken.ShutdownToken ?? default;

    private async Task<IResult> RunAsync(ICommand command, CancellationToken ct)
    {
        IResult response;
        Exception? err = null;

        var meta = Context.BackgroundJob;
        try
        {
            _logger?.LogDebug("Start process command: {@Command}", command);
            _logger?.LogInformation("Start process {Job} command: {Name}", meta.Id, command.GetName());

            response = await _dispatcher.DispatchDynamicAsync(_provider, command, ct);
        }
        catch (Exception exc)
        {
            err = exc;
            _logger?.LogError(exc, "Error processing jobId: {JobId}, command: {@Command}", meta.Id, command);

            if (_onError is not null)
                await _onError(command, exc);

            var error = command.ErrorResponse(_errorBody);
            response = Result.FromError<object>(error);
        }

        if (_completionService is not null)
            response = await _completionService.CompleteAsync(command, response, err, CancellationToken);

        _logger?.LogDebug("End process JobId: {JobId} command: {@Command} Response: {@Response}", meta.Id, command, response);
        LogCompleteProcess(meta, err is not null || response.Error is not null);

        return response;
    }
    private void LogCompleteProcess(BackgroundJob meta, bool hasError)
    {
        if (!hasError)
        {
            _logger?.LogInformation("End process {Job} successfully", meta.Id);
            return;
        }
        _logger?.LogWarning("End process {Job} with error", meta.Id);
    }
    #endregion
}
