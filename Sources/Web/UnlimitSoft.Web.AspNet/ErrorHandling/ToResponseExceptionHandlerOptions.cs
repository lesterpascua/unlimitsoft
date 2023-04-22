using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.Web.AspNet.Filter;

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
    private readonly LogLevel _logLevel;
    private readonly IJsonSerializer _serializer;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="showExceptionInfo"></param>
    /// <param name="level">Level of the log.</param>
    /// <param name="handlers"></param>
    /// <param name="fatalErrorCode"></param>
    /// <param name="errorBody"></param>
    /// <param name="contentType">Content type in the response by default application/json</param>
    /// <param name="logger"></param>
    public ToResponseExceptionHandlerOptions(
        IJsonSerializer serializer,
        bool showExceptionInfo = true,
        LogLevel level = LogLevel.Information,
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
        _logLevel = level;
        ExceptionHandler = ExceptionHandlerInternal;
    }

    /// <summary>
    /// Get response depending of the exception
    /// </summary>
    /// <param name="context"></param>
    /// <param name="feature"></param>
    /// <returns></returns>
    private IResponse GetResponse(HttpContext context, IExceptionHandlerFeature feature)
    {
        if (feature.Error is ResponseException exception)
            return new Response<IDictionary<string, string[]>>(exception.Code, exception.Body);

        object body = feature.Error;
        if (!_showExceptionInfo)
            body = _errorBody?.Invoke(context) ?? _defaultErrorBody;

        return new Response<object>(HttpStatusCode.InternalServerError, body);
    }

    #region Private Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private async Task ExceptionHandlerInternal(HttpContext context)
    {
        var identity = context.User.Identity;
#if NET7_0_OR_GREATER
        var feature = context.Features.GetRequiredFeature<IExceptionHandlerFeature>();
#else 
        var feature = context.Features.Get<IExceptionHandlerFeature>() ?? throw new InvalidOperationException($"Feature '{typeof(IExceptionHandlerFeature)}' is not present.");
#endif

        _logger?.LogError(feature!.Error, "User: {Name}, logged in from: {IpAddress}", identity?.Name, context.GetIpAddress());
        if (_handlers is not null)
            foreach (var handler in _handlers.Where(x => x.ShouldHandle(context)))
                await handler.HandleAsync(context);

        var response = GetResponse(context, feature);
        context.Response.ContentType = _contentType;
        context.Response.StatusCode = (int)response.Code;


        var traceId = context.TraceIdentifier;
        var correlationId = context.TraceIdentifier;
        //var isTrusted = _trusted is null || _trusted.IsTrustedRequest(context);
        //if (isTrusted && context.Request.Headers.TryGetValue(SysContants.HeaderCorrelation, out var correlationFromHeader))
        //    correlationId = correlationFromHeader;
        context.Response.Headers.TryAdd(SysContants.HeaderTrace, traceId);
        context.Response.Headers.TryAdd(SysContants.HeaderCorrelation, correlationId);

        var json = _serializer.Serialize(response);
        if (json is not null)
            await context.Response.WriteAsync(json);

        _logger?.Log(_logLevel, RequestLoggerAttribute.ResponseTemplate, response);
    }
    #endregion
}
