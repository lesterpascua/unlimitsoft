using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
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
    private readonly ProcessCommandMiddleware? _middleware;
    private readonly ILogger<DefaultJobProcessor<TProps>>? _logger;

    private static readonly JsonSerializerOptions _deserializerJsonSettings;


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
    /// <param name="middleware">Allow to invoke some action before the command processing.</param>
    /// <param name="logger"></param>
    public DefaultJobProcessor(
        IServiceProvider provider,
        ICommandDispatcher dispatcher,
        ProcessCommandMiddleware? middleware = null,
        ILogger<DefaultJobProcessor<TProps>>? logger = null
    )
    {
        _logger = logger;
        _provider = provider;
        _dispatcher = dispatcher;
        _middleware = middleware;

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

    private ValueTask<IResult> RunAsync(ICommand command, CancellationToken ct)
    {
        var jobId = Context.BackgroundJob.Id;
        _logger?.LogDebug("Start process JobId: {JobId} command: {@Command}", jobId, command);

        return _dispatcher.DispatchDynamicAsync(_provider, command, ct);
    }
    #endregion
}
