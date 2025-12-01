using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UnlimitSoft.Mediator;

namespace Mediator;


/// <summary>
/// 
/// </summary>
public sealed class DemoHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IMediator _mediator;
    private readonly ILogger<DemoHostedService> _logger;

    private Task? _jobTask;


    /// <summary>
    /// Initializes a new instance of the DemoHostedService class with the specified application lifetime manager.
    /// </summary>
    /// <param name="appLifetime">The application lifetime manager used to handle application start and stop events. Cannot be null.</param>
    public DemoHostedService(IHostApplicationLifetime appLifetime, IMediator mediator, ILogger<DemoHostedService> logger)
    {
        _appLifetime = appLifetime;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken ct)
    {
        _jobTask = Task.Run(async () =>
        {
            try
            {
                await OnStartedAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in {ServiceName}", nameof(DemoHostedService));
            }
        }, ct);
        return Task.CompletedTask;
    }
    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task OnStartedAsync(CancellationToken ct)
    {
        _logger.LogInformation("Mediator request started");

        var request = new TestRequest { Message = "Hello from DemoHostedService!" };
        var response = await _mediator.SendAsync(request, ct);
        _logger.LogProcessRequest(response.Value);
    }
}
internal static partial class DemoHostedServiceLogger
{
    [LoggerMessage(Message = "Mediator message '{Message}'", Level = LogLevel.Information)]
    public static partial void LogProcessRequest(this ILogger logger, string? message);
}