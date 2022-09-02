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

        object headers = null;
        if (_settings.AddHeader)
        {
            var aux = context.HttpContext.Request.Headers?.ToDictionary(k => k.Key, v => v.Value);
            if (_settings.Transform is not null)
                aux = _settings.Transform(aux);
            headers = aux;
        }

        _logger.Log(_settings.LogLevel, "User: {User}, Request: {@Request}",
            sub,
            new
            {
                Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                Address = httpContext.GetIpAddress(),
                Headers = headers,
                Body = context.ActionArguments,
            });
    }
    /// <inheritdoc />
    public override void OnResultExecuted(ResultExecutedContext context)
    {
        if (_settings.LogLevel == LogLevel.None || !_logger.IsEnabled(_settings.LogLevel))
            return;

        var httpContext = context.HttpContext;
        var sub = httpContext.User.GetSubjectId();
        if (!string.IsNullOrEmpty(sub) && Guid.TryParse(sub, out var subGuid))
            sub = subGuid.ToString();

        _logger.Log(_settings.LogLevel, "User: {User}, Response: {@Response}",
            sub,
            context.Result is ObjectResult result ? result.Value : context.Result
        );
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
