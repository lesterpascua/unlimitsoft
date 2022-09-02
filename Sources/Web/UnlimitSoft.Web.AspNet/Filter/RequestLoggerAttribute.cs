using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnlimitSoft.Web.Security.Claims;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

namespace UnlimitSoft.Web.AspNet.Filter;


/// <summary>
/// 
/// </summary>
public class RequestLoggerAttribute : ActionFilterAttribute
{
    private readonly Settings _settings;
    private readonly ILogger<RequestLoggerAttribute> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public RequestLoggerAttribute(IOptions<Settings> options, ILogger<RequestLoggerAttribute> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (_settings.LogLevel == LogLevel.None || !_logger.IsEnabled(_settings.LogLevel))
            return;

        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        var sub = httpContext.User.GetSubjectId();
        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var subGuid))
            sub = subGuid.ToString();

        Dictionary<string, StringValues> headers = null;
        if (_settings.AddHeader)
        {
            headers = context.HttpContext.Request.Headers?.ToDictionary(k => k.Key, v => v.Value);
            if (_settings.Transform is not null)
                headers = _settings.Transform(headers);
        }

        var url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
        _logger.Log(_settings.LogLevel, "Request from {Address}, {Method} {Url} Body = {@Arguments}, Header = {@Headers}",
            httpContext.GetIpAddress(),
            request.Method,
            url,
            context.ActionArguments,
            headers
        );
    }
    /// <inheritdoc />
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        if (_settings.LogLevel == LogLevel.None || !_logger.IsEnabled(_settings.LogLevel))
            return;

        _logger.Log(_settings.LogLevel, "Response {@Response}", context.Result is ObjectResult result ? result.Value : context.Result);
    }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public class Settings
    {
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
        public Func<Dictionary<string, StringValues>, Dictionary<string, StringValues>> Transform { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
    #endregion
}
