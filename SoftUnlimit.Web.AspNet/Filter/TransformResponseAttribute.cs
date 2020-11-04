using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Web.Client;
using SoftUnlimit.Web.Security.Claims;
using System;
using System.Collections.Generic;

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
                _logger?.LogError(context.Exception, "ConnectionId: {Id}, Code {Code}, user: {User}, ip: {Ip}", httpContext.Connection.Id, 500, httpContext.User.GetSubjectId(), httpContext.GetIpAddress());

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
            /// 
            /// </summary>
            public bool ShowExceptionDetails { get; set; } = false;
        }
        #endregion
    }
}
