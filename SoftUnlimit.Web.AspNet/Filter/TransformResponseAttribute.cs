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
using System.Linq;
using System.Text.Json;

namespace SoftUnlimit.Web.AspNet.Filter
{
    /// <summary>
    /// 
    /// </summary>
    public class TransformResponseAttribute : ActionFilterAttribute
    {
        private IDictionary<string, object> _requestArguments;
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

        /// <inheritdoc />
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _requestArguments = context.ActionArguments;
        }
        /// <inheritdoc />
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var httpContext = context.HttpContext;
            if (context.Exception != null && context.Controller is ControllerBase controller)
            {
                _settings.ErrorLogger?.Invoke(_logger, context.Exception, httpContext, _requestArguments);

                Response<object> response;
                if (!(context.Exception is ResponseException exc))
                {
                    object body = context.Exception;
                    if (!_settings.ShowExceptionDetails)
                        body = new Dictionary<string, string[]> {
                            { string.Empty, new string[] { _settings.ServerErrorText } }
                        };

                    response = new Response<object> {
                        TraceIdentifier = httpContext.TraceIdentifier,
                        IsSuccess = false,
                        Code = StatusCodes.Status500InternalServerError,
                        Body = body,
                        UIText = _settings.ServerErrorText
                    };
                } else
                    response = new Response<object> {
                        TraceIdentifier = httpContext.TraceIdentifier,
                        IsSuccess = false,
                        Code = exc.Code,
                        Body = exc.Body,
                        UIText = exc.Message
                    };

                context.Result = controller.StatusCode(StatusCodes.Status500InternalServerError, response);
                context.Exception = null;
            } else
                _settings.ResponseLogger?.Invoke(_logger, httpContext, context.Result);
        }
        /// <summary>
        /// Convert fluent error in Response error.
        /// </summary>
        /// <param name="context"></param>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is BadRequestObjectResult result && result.Value is ValidationProblemDetails validationProblem)
            {
                var response = new Response<IDictionary<string, string[]>> {
                    TraceIdentifier = context.HttpContext.TraceIdentifier,
                    IsSuccess = false,
                    Code = result.StatusCode.Value,
                    Body = validationProblem.Errors,
                    UIText = _settings.InvalidArgumentText
                };

                _settings.BadRequestLogger?.Invoke(_logger, context.HttpContext, response, _settings.InvalidArgumentText);
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
            public string ServerErrorText { get; set; } = "An error occurred while processing your request. Submit this bug and we'll fix it.";
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
            public Action<ILogger, Exception, HttpContext, IDictionary<string, object>> ErrorLogger { get; set; } = (logger, exception, httpContext, requestArguments) => {
                var request = httpContext.Request;
                logger.LogError(exception, "User: {User}, Response: {@Response}",
                    httpContext.User.GetSubjectId(),
                    new {
                        Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                        Address = httpContext.GetIpAddress(),
                        RequestArguments = requestArguments
                    });
            };
            /// <summary>
            /// Log bad request exception
            /// </summary>
            public Action<ILogger, HttpContext, object, string> BadRequestLogger { get; set; } = (logger, httpContext, response, message) => logger.LogInformation("Code: {Code}, User: {User}, Message: {Message}, Response: {@Response}",
                StatusCodes.Status400BadRequest,
                httpContext.User.GetSubjectId(),
                message,
                response
            );
            /// <summary>
            /// Log error exception
            /// </summary>
            public Action<ILogger, HttpContext, object> ResponseLogger { get; set; } = (logger, httpContext, response) => logger.LogInformation("Code: {Code}, User: {User}, Response: {@Response}",
                response is ObjectResult result ? result.StatusCode ?? StatusCodes.Status200OK : StatusCodes.Status200OK,
                httpContext.User.GetSubjectId(),
                response is ObjectResult ? response : "Raw Data"
            );
        }
        #endregion
    }
}
