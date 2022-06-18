using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using SoftUnlimit.Logger.Logging;
using System.Threading.Tasks;

namespace SoftUnlimit.Logger.Web;


/// <summary>
/// Add logger correlation and traceId. Can add custom properties.
/// </summary>
public class LoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggerMiddleware> _logger;

    /// <summary>
    /// Header asociate to correlation id if the header is not present use TraceId
    /// </summary>
    public const string CorrelationHeader = "X-Correlation-ID";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    public LoggerMiddleware(RequestDelegate next, ILogger<LoggerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.TraceIdentifier;
        if (context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationHeader))
            correlationId = correlationHeader;

        using var _1 = LogContext.PushProperty("CorrelationId", correlationId);
        using var _2 = LogContext.PushProperty("TraceId", context.TraceIdentifier);

        // Log the asociation to historical debuging process
        if (context.TraceIdentifier != correlationId)
            _logger.AssociateTraceWithCorrelation(context.TraceIdentifier, correlationId);

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }
}