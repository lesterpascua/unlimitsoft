using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftUnlimit.Web.Security.Claims;
using System;
using System.Linq;
using System.Text.Json;

namespace SoftUnlimit.Web.AspNet.Filter
{
    /// <summary>
    /// Validate model and result from endpoint entry. Review <see cref="SkipValidationModelAttribute"/> to
    /// skip global filter.
    /// </summary>
    public class ValidationModelAttribute : ActionFilterAttribute
    {
        private readonly Settings _settings;
        private readonly ILogger<ValidationModelAttribute> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ValidationModelAttribute(IOptions<Settings> options, ILogger<ValidationModelAttribute> logger = null)
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
            if (context.Controller is ControllerBase controller && !context.ModelState.IsValid && !SkipDependingOfAttribute<SkipValidationModelAttribute>(context.ActionDescriptor as ControllerActionDescriptor))
            {
                context.Result = controller.BadRequest(context.ModelState);

                if (_settings.LogLevel != LogLevel.None)
                {
                    var httpContext = context.HttpContext;
                    _logger?.Log(_settings.LogLevel, "TraceId: {Trace}, Code: {Code}, User: {@User}", httpContext.TraceIdentifier, StatusCodes.Status400BadRequest, httpContext.User.GetSubjectId());
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Check if funtion has attribute specific.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        protected bool SkipDependingOfAttribute<TAttribute>(ControllerActionDescriptor descriptor) where TAttribute : Attribute
        {
            if (descriptor == null || !descriptor.MethodInfo.GetCustomAttributes(typeof(TAttribute), true).Any())
                return false;
            return true;
        }

        #endregion

        #region Nested Classes
        /// <summary>
        /// 
        /// </summary>
        public class Settings
        {
            /// <summary>
            /// 
            /// </summary>
            public LogLevel LogLevel { get; set; } = LogLevel.Debug;
        }
        #endregion
    }
}
