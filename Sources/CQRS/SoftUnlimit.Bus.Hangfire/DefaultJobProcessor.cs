using Hangfire;
using Microsoft.Extensions.Logging;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Bus.Hangfire;


/// <summary>
/// Default job processor.
/// </summary>
public class DefaultJobProcessor : IJobProcessor
{
    private readonly IServiceProvider _provider;
    private readonly ICommandDispatcher _dispatcher;
    private readonly Func<Exception, Task> _onError;
    private readonly ICommandCompletionService _completionService;
    private readonly Func<IServiceProvider, ICommand, BackgroundJob, Func<ICommand, CancellationToken, Task<ICommandResponse>>, CancellationToken, Task<ICommandResponse>> _preeProcess;
    private readonly ILogger<DefaultJobProcessor> _logger;

    private readonly string _errorCode;
    private readonly Dictionary<string, string[]> _errorBody;


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
        string errorCode = "-1",
        Func<Exception, Task> onError = null,
        ICommandCompletionService completionService = null,
        Func<IServiceProvider, ICommand, BackgroundJob, Func<ICommand, CancellationToken, Task<ICommandResponse>>, CancellationToken, Task<ICommandResponse>> preeProcess = null,
        ILogger<DefaultJobProcessor> logger = null
    )
    {
        _logger = logger;
        _provider = provider;
        _dispatcher = dispatcher;
        _onError = onError;
        _preeProcess = preeProcess;
        _completionService = completionService;

        _errorCode = errorCode;
        _errorBody = new Dictionary<string, string[]> { [string.Empty] = new string[] { _errorCode } };
    }

    /// <summary>
    /// 
    /// </summary>
    public BackgroundJob Metadata { get; set; }
    /// <summary>
    /// Cancelation token for this job.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <inheritdoc />
    public async Task<ICommandResponse> ProcessAsync(string json, Type type)
    {
        var command = (ICommand)JsonUtility.Deserialize(type, json);
        if (_preeProcess is null)
            return await RunAsync(command, CancellationToken);

        return await _preeProcess(_provider, command, Metadata, RunAsync, CancellationToken);
    }

    #region Private Methods
    private async Task<ICommandResponse> RunAsync(ICommand command, CancellationToken ct)
    {
        Exception err = null;
        ICommandResponse response;
        var props = command.GetProps<CommandProps>();
        try
        {
            _logger.LogDebug("Start process command: {@Command}", command);
            _logger.LogInformation("Start process {Job} command: {Id}", Metadata.Id, props.Id);

            response = await _dispatcher.DispatchAsync(_provider, command, ct);
        }
        catch (Exception exc)
        {
            err = exc;
            _logger.LogError(exc, "Error processing jobId: {JobId}, command: {@Command}", Metadata.Id, command);

            if (_onError != null)
                await _onError(exc);
            response = command.ErrorResponse(_errorBody);
        }

        if (_completionService is not null && (!props.Silent || !response.IsSuccess))
            await _completionService.CompleteAsync(command, response, err, CancellationToken);

        _logger.LogDebug(@"End process
JobId: {JobId}
command: {@Command}
Response: {@Response}", Metadata.Id, command, response);
        _logger.LogInformation("End process {Job} with error {Error}", Metadata.Id, err is not null || response?.IsSuccess == false);

        return response;
    }
    #endregion
}
