using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Threading.Tasks;

namespace UnlimitSoft.Logger.AspNet;


/// <summary>
/// Add logger correlation and traceId. Can add custom properties.
/// </summary>
public class LoggerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationTrusted? _trusted;
    private readonly ILogger<LoggerMiddleware>? _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="trusted"></param>
    /// <param name="logger"></param>
    public LoggerMiddleware(RequestDelegate next, ICorrelationTrusted? trusted = null, ILogger<LoggerMiddleware>? logger = null)
    {
        _next = next;
        _trusted = trusted;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public virtual async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;
        var correlationId = context.TraceIdentifier;

        var isTrusted = _trusted is null || _trusted.IsTrustedRequest(context);
        if (isTrusted && context.Request.Headers.TryGetValue(SysContants.HeaderCorrelation, out var correlationFromHeader))
            correlationId = correlationFromHeader;

        context.Response.Headers.Add(SysContants.HeaderTrace, traceId);
        context.Response.Headers.Add(SysContants.HeaderCorrelation, correlationId);

        using var _1 = LogContext.PushProperty(SysContants.LogContextCorrelationId, correlationId);

        // Log the asociation to historical debuging process
        if (traceId != correlationId)
            _logger?.LogInformation("Associate {Trace} with {Correlation}", traceId, correlationId);

        // Call the next delegate/middleware in the pipeline.
        await _next(context);
    }
}