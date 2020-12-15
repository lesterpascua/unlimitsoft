using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;

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

                var log = new {
                    Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                    User = new {
                        httpContext.User.Identity.Name,
                        IsAuth = httpContext.User.Identity.IsAuthenticated,
                        AuthType = httpContext.User.Identity.AuthenticationType,
                        Claims = httpContext.User.Claims.Select(s => new {
                            s.Type,
                            s.Value,
                            s.Issuer
                        })
                    },
                    Address = httpContext.GetIpAddress(),
                    Body = context.ActionArguments,
                    ConnectionId = httpContext.Connection.Id,
                    TraceId = httpContext.TraceIdentifier
                };
                var jsonLog = JsonSerializer.Serialize(log, new JsonSerializerOptions { WriteIndented = _settings.Indented });
                _logger.Log(_settings.LogLevel, jsonLog);
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
