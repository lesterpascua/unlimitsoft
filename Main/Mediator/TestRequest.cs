using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;

namespace Mediator;


/// <summary>
/// 
/// </summary>
public sealed class TestRequest : IRequest<string>
{
    /// <summary>
    /// Message to be processed.
    /// </summary>
    public required string Message { get; init; }
}
internal sealed class TestRequestHandler : IRequestHandler<TestRequest, string>, IRequestHandlerLifeCycle<TestRequest>, IRequestHandlerValidator<TestRequest>, IRequestHandlerCompliance<TestRequest>
{
    private readonly ILogger<TestRequestHandler> _logger;

    public TestRequestHandler(ILogger<TestRequestHandler> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public ValueTask InitAsync(TestRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("1 - {Action}", nameof(InitAsync));
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc />
    public ValueTask<string> HandleAsync(TestRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("4 - {Action}", nameof(HandleAsync));
        return ValueTask.FromResult($"Processed: {request.Message}");
    }
    /// <inheritdoc />
    public ValueTask<IResponse> ComplianceAsync(TestRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("3 - {Action}", nameof(ComplianceAsync));
        return ValueTask.FromResult(request.OkResponse());
    }
    /// <inheritdoc />
    public ValueTask<IResponse> ValidatorAsync(TestRequest request, RequestValidator<TestRequest> validator, CancellationToken ct = default)
    {
        _logger.LogInformation("2 - {Action}", nameof(ValidatorAsync));
        return ValueTask.FromResult(request.OkResponse());
    }
    /// <inheritdoc />
    public ValueTask EndAsync(TestRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("5 - {Action}", nameof(EndAsync));
        return ValueTask.CompletedTask;
    }
}
internal sealed class TestRequestHandlerTimerCounter : IRequestHandlerPipeline<TestRequest, string>
{
    private readonly ILogger<TestRequestHandlerTimerCounter> _logger;

    private long _start;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public TestRequestHandlerTimerCounter(ILogger<TestRequestHandlerTimerCounter> logger)
    {
        _logger = logger;
    }

    public int Order => 0;

    /// <inheritdoc />
    public ValueTask PreHandleAsync(TestRequest command, CancellationToken ct)
    {
        _start = Stopwatch.GetTimestamp();
        return ValueTask.CompletedTask;
    }
    /// <inheritdoc />
    public ValueTask<string> PostHandleAsync(TestRequest command, string response, CancellationToken ct)
    {
        var elapsed = Stopwatch.GetElapsedTime(_start);
        _logger.LogInformation("{Request} was executed in {Time}", nameof(TestRequest), elapsed);
        return ValueTask.FromResult(response);
    }
}