using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Json;
using SoftUnlimit.Web.Client;
using SoftUnlimit.Web.Security.Claims;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SoftUnlimit.Web.AspNet.Filter
{
    /// <summary>
    /// 
    /// </summary>
    public class TransformResponseAttribute : ActionFilterAttribute
    {
        private readonly Settings _settings;
        private readonly ILogger<TransformResponseAttribute> _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public TransformResponseAttribute(IOptions<Settings> options, ILogger<TransformResponseAttribute> logger = null)
        {
            _settings = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null && context.Controller is ControllerBase controller)
            {
                var httpContext = context.HttpContext;
                _settings.ErrorLogger?.Invoke(_logger, context.Exception, httpContext, _settings.ServerErrorText);

                object body = context.Exception;
                if (!_settings.ShowExceptionDetails)
                    body = new Dictionary<string, string[]> {
                        { string.Empty, new string[] { _settings.ServerErrorText } }
                    };

                var response = new Response<object> {
                    IsSuccess = false,
                    Code = StatusCodes.Status500InternalServerError,
                    Body = body,
                    UIText = _settings.ServerErrorText
                };
                context.Result = controller.StatusCode(StatusCodes.Status500InternalServerError, response);
                context.Exception = null;
            }
        }
        /// <summary>
        /// Convert fluent error in Response error.
        /// </summary>
        /// <param name="context"></param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is BadRequestObjectResult result && result.Value is ValidationProblemDetails validationProblem)
            {
                _settings.BadRequestLogger?.Invoke(_logger, context.HttpContext, _settings.InvalidArgumentText);

                var response = new Response<IDictionary<string, string[]>> {
                    IsSuccess = false,
                    Code = result.StatusCode.Value,
                    Body = validationProblem.Errors,
                    UIText = _settings.InvalidArgumentText
                };
                context.Result = new BadRequestObjectResult(response);
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
            public string ServerErrorText { get; set; } = "Server Error";
            /// <summary>
            /// 
            /// </summary>
            public string InvalidArgumentText { get; set; } = "Invalid Argument";
            /// <summary>
            /// Indicate if the filter should log exception request.
            /// </summary>
            public bool ShowExceptionDetails { get; set; } = false;
            /// <summary>
            /// Log error exception
            /// </summary>
            public Action<ILogger, Exception, HttpContext, string> ErrorLogger { get; set; } = (logger, exception, httpContext, message) => logger.LogErrorJson(new {
                TraceId = httpContext.TraceIdentifier, 
                Code = StatusCodes.Status500InternalServerError, 
                UserId = httpContext.User.GetSubjectId(),
                Message = message
            }, exception);
            /// <summary>
            /// Log bad request exception
            /// </summary>
            public Action<ILogger, HttpContext, string> BadRequestLogger { get; set; } = (logger, httpContext, message) => logger.LogInformationJson(new {
                TraceId = httpContext.TraceIdentifier,
                Code = StatusCodes.Status400BadRequest,
                UserId = httpContext.User.GetSubjectId(),
                Message = message
            });
        }
        #endregion
    }
}
