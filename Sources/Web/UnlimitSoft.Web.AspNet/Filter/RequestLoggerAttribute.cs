﻿using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnlimitSoft.Web.Security.Claims;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace UnlimitSoft.Web.AspNet.Filter
{
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

            _logger.Log(_settings.LogLevel, "User: {User}, Request: {@Request}",
                sub,
                new
                {
                    Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                    Address = httpContext.GetIpAddress(),
                    Body = context.ActionArguments,
                    Headers = context.HttpContext.Request.Headers?.ToDictionary(k => k.Key, v => v.Value)
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
            /// 
            /// </summary>
            public LogLevel LogLevel { get; set; } = LogLevel.Information;
        }
        #endregion
    }
}
