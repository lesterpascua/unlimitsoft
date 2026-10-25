using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using UnlimitSoft.Web.Security.Claims;

namespace UnlimitSoft.Web.AspNet.Filter;


/// <summary>
/// Validate model and result from endpoint entry. Review <see cref="SkipValidationModelAttribute"/> to
/// skip global filter.
/// </summary>
public class ValidationModelAttribute : ActionFilterAttribute
{
    private readonly Settings _settings;
    private readonly ILogger<ValidationModelAttribute>? _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ValidationModelAttribute(IOptions<Settings> options, ILogger<ValidationModelAttribute>? logger = null)
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

            if (_logger is not null && _settings.LogLevel != LogLevel.None)
            {
                var httpContext = context.HttpContext;
                var user = httpContext.User.GetSubjectId();
                _logger.InvalidModel(_settings.LogLevel, httpContext.TraceIdentifier, StatusCodes.Status400BadRequest, user);
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
    protected static bool SkipDependingOfAttribute<TAttribute>(ControllerActionDescriptor? descriptor) where TAttribute : Attribute
    {
        if (descriptor is null || !descriptor.MethodInfo.GetCustomAttributes(typeof(TAttribute), true).Any())
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
internal static partial class ValidationModelAttributeLoggerExtensions
{
    [LoggerMessage(EventId = 1, Message = "TraceId: {Trace}, Code: {Code}, User: {User}")]
    public static partial void InvalidModel(this ILogger<ValidationModelAttribute> logger, LogLevel level, string trace, int code, string user);
}
