using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.AspNet.ErrorHandling;


/// <summary>
/// Transform exception to respond
/// </summary>
public class ToResponseExceptionHandlerOptions : ExceptionHandlerOptions
{
    private readonly bool _showExceptionInfo;
    private readonly IEnumerable<IExceptionHandler>? _handlers;
    private readonly Dictionary<string, string[]> _defaultErrorBody;
    private readonly ILogger<ToResponseExceptionHandlerOptions>? _logger;
    private readonly Func<HttpContext, Dictionary<string, string[]>>? _errorBody;
    private readonly string _contentType;
    private readonly IJsonSerializer _serializer;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="showExceptionInfo"></param>
    /// <param name="handlers"></param>
    /// <param name="fatalErrorCode"></param>
    /// <param name="errorBody"></param>
    /// <param name="logger"></param>
    public ToResponseExceptionHandlerOptions(
        IJsonSerializer serializer,
        bool showExceptionInfo = true,
        IEnumerable<IExceptionHandler>? handlers = null,
        int fatalErrorCode = -1,
        Func<HttpContext, Dictionary<string, string[]>>? errorBody = null,
        string contentType = "application/json",
        ILogger<ToResponseExceptionHandlerOptions>? logger = null
    )
    {
        _serializer = serializer;
        _showExceptionInfo = showExceptionInfo;
        _logger = logger;
        _handlers = handlers;
        _defaultErrorBody = new Dictionary<string, string[]> { { string.Empty, new string[] { fatalErrorCode.ToString() } } };
        _errorBody = errorBody;
        _contentType = contentType;
        ExceptionHandler = ExceptionHandlerInternal;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task ExceptionHandlerInternal(HttpContext context)
    {
        var identity = context.User.Identity;
        var feature = context.Features.Get<IExceptionHandlerFeature>();

        _logger?.LogError(feature!.Error, "User: {Name}, logged in from: {IpAddress}", identity?.Name, context.GetIpAddress());
        if (_handlers != null)
            foreach (var handler in _handlers.Where(x => x.ShouldHandle(context)))
                await handler.HandleAsync(context);

        object body = feature!.Error;
        if (!_showExceptionInfo)
            body = _errorBody?.Invoke(context) ?? _defaultErrorBody;

        var response = new Response<object>(HttpStatusCode.InternalServerError, body);

        context.Response.ContentType = _contentType;
        context.Response.StatusCode = (int)response.Code;


        var traceId = context.TraceIdentifier;
        var correlationId = context.TraceIdentifier;
        //var isTrusted = _trusted is null || _trusted.IsTrustedRequest(context);
        //if (isTrusted && context.Request.Headers.TryGetValue(SysContants.HeaderCorrelation, out var correlationFromHeader))
        //    correlationId = correlationFromHeader;
        context.Response.Headers.Add(SysContants.HeaderTrace, traceId);
        context.Response.Headers.Add(SysContants.HeaderCorrelation, correlationId);

        var json = _serializer.Serialize(response)!;
        await context.Response.WriteAsync(json);
    }
}
