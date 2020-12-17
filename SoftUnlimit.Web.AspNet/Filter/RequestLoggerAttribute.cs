using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SoftUnlimit.Web.Security.Claims;
using System;
using System.Linq;

namespace SoftUnlimit.Web.AspNet.Filter
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (_settings.LogLevel != LogLevel.None)
            {
                var httpContext = context.HttpContext;
                var request = httpContext.Request;

                _logger.Log(_settings.LogLevel, "TraceId: {Trace}, User: {@User}, JsonData: {@Response}",
                    httpContext.TraceIdentifier,
                    new {
                        IsAuth = httpContext.User.Identity.IsAuthenticated,
                        AuthType = httpContext.User.Identity.AuthenticationType,
                        Claims = httpContext.User.Claims.Select(s => $"{s.Type}={s.Value}, ")
                    },
                    new {
                        Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                        Address = httpContext.GetIpAddress(),
                        Body = context.ActionArguments
                    });
            }
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
