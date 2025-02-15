using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Security.Claims;

namespace UnlimitSoft.Web.AspNet.Filter;


/// <summary>
/// Log the request and the response of the API 
/// </summary>
public sealed class RequestLoggerAttribute : ActionFilterAttribute
{
    private readonly Options _options;
    private readonly ITrustedIPResolver? _ipResolver;
    private readonly ILogger<RequestLoggerAttribute> _logger;

    /// <summary>
    /// Template used for response
    /// </summary>
    public const string ResponseTemplate = "API Response {TraceId} Code = {Code}, Body = {@Response}";
    /// <summary>
    /// Template used to log request
    /// </summary>
    public const string RequestTemplate = "API Request {TraceId} from {Address}, {Method} {Url} Body = {@Arguments}, Header = {@Headers}";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="ipResolver"></param>
    /// <param name="logger"></param>
    public RequestLoggerAttribute(IOptions<Options> options, ITrustedIPResolver? ipResolver, ILogger<RequestLoggerAttribute> logger)
    {
        _options = options.Value;
        _ipResolver = ipResolver;
        _logger = logger;
    }

    /// <inheritdoc />
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is null || _options.LogLevel == LogLevel.None || !_logger.IsEnabled(_options.LogLevel))
            return;

        int code = 500;
        object result = context.Exception.Message;
        if (context.Exception is ResponseException ex)
        {
            result = ex.Body;
            code = (int)ex.Code;
        }

        _logger.Log(_options.LogLevel, ResponseTemplate, context.HttpContext.TraceIdentifier, code, result);
    }
    /// <inheritdoc />
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        if (_options.LogLevel == LogLevel.None || !_logger.IsEnabled(_options.LogLevel))
            return;

        int? code = null;
        object? response = context.Result;
        if (context.Result is ObjectResult result)
        {
            response = result.Value;
            code = result.StatusCode;
        }
        _logger.Log(_options.LogLevel, ResponseTemplate, context.HttpContext.TraceIdentifier, code, response);
    }
    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (_options.LogLevel == LogLevel.None || !_logger.IsEnabled(_options.LogLevel))
            return;

        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        var sub = httpContext.User.GetSubjectId();
        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var subGuid))
            sub = subGuid.ToString();

        Dictionary<string, StringValues>? headers = null;
        if (_options.AddHeader)
        {
            headers = context.HttpContext.Request.Headers?.ToDictionary(k => k.Key, v => v.Value);
            if (_options.Transform is not null && headers is not null)
                headers = _options.Transform(headers);
        }

        var actionArguments = GetActionArguments(context);
        var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        var ip = _ipResolver?.GetIPAddress(httpContext) ?? httpContext.GetIpAddress();

        _logger.Log(_options.LogLevel, RequestTemplate, httpContext.TraceIdentifier, ip, request.Method, url, actionArguments, headers);
    }

    #region Private Methods
    private IEnumerable<KeyValuePair<string, object?>> GetActionArguments(ActionExecutingContext context)
    {
        return context.ActionArguments
            .Where(arg =>
            {
                if (arg.Value is null)
                    return false;

                var param = context.ActionDescriptor.Parameters.First(p => p.Name == arg.Key);
                if (param is ControllerParameterDescriptor descriptor)
                {
                    if (descriptor.ParameterInfo.IsDefined(typeof(FromQueryAttribute), false))
                        return false;
                    if (_options.Ignore is not null && descriptor.ParameterInfo.IsDefined(_options.Ignore, false))
                        return false;
                };
                return true;
            });
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public sealed class Options
    {
        /// <summary>
        /// Specified attribute type to ignode some parameters into the logger. All parameter with this attribute will be ignored and not logger
        /// </summary>
        public Type? Ignore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool Indented { get; set; } = false;
        /// <summary>
        /// Include headers in the log
        /// </summary>
        public bool AddHeader { get; set; } = false;
        /// <summary>
        /// If is not null transform the header to be logged (use to remove sencitive information)
        /// </summary>
        public Func<Dictionary<string, StringValues>, Dictionary<string, StringValues>>? Transform { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
    #endregion
}