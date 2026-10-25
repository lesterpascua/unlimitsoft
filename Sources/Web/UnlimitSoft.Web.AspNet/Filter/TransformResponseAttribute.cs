using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Security.Claims;

namespace UnlimitSoft.Web.AspNet.Filter;


/// <summary>
/// Transform 400 error into the proper dictionary format and add identifier to all the 200 response.
/// </summary>
public sealed class TransformResponseAttribute : ActionFilterAttribute
{
    private readonly TransformResponseAttributeOptions _options;
    private readonly ILogger<TransformResponseAttribute>? _logger;

    /// <summary>
    /// Header used to sent the identifier of the operation
    /// </summary>
    public const string XTraceId = "X-TraceId";


    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public TransformResponseAttribute(IOptions<TransformResponseAttributeOptions> options, ILogger<TransformResponseAttribute>? logger = null)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Convert fluent error in Response error.
    /// </summary>
    /// <param name="context"></param>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (_options.HandlerException is not null)
        {
            _options.HandlerException(context);
            return;
        }

        switch (context.Result)
        {
            case BadRequestObjectResult result:
                if (result.Value is ValidationProblemDetails validationProblem)
                {
                    var code = (HttpStatusCode?)result.StatusCode ?? HttpStatusCode.BadRequest;
                    result.Value = new ErrorResponse(code, validationProblem.Errors);

                    _options.BadRequestLogger?.Invoke(_logger, context.HttpContext, result.Value, "InvalidArgument");
                }
                break;
        }
    }
}
/// <summary>
/// 
/// </summary>
public class TransformResponseAttributeOptions
{
    /// <summary>
    /// 
    /// </summary>
    public string ServerErrorText { get; set; } = "An error occurred while processing your request. Submit this bug and we'll fix it.";
    /// <summary>
    /// Indicate if the filter should log exception request.
    /// </summary>
    public bool ShowExceptionDetails { get; set; } = false;


    /// <summary>
    /// Log error exception
    /// </summary>
    public Action<ILogger, Exception, HttpContext, IDictionary<string, object>> ErrorLogger { get; set; } = (logger, exception, httpContext, requestArguments) =>
    {
        var request = httpContext.Request;
        logger.ErrorResponse(exception,
            httpContext.User.GetSubjectId(),
            new
            {
                Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}",
                Address = httpContext.GetIpAddress(),
                RequestArguments = requestArguments
            });
    };
    /// <summary>
    /// Log bad request exception
    /// </summary>
    public Action<ILogger?, HttpContext, object, string> BadRequestLogger { get; set; } = (logger, httpContext, response, message) =>
    {
        if (logger is null || !logger.IsEnabled(LogLevel.Information))
            return;

        logger.BadRequestResponse(
            StatusCodes.Status400BadRequest,
            httpContext.User.GetSubjectId(),
            message,
            response
        );
    };
    /// <summary>
    /// Log error exception
    /// </summary>
    public Action<ILogger, HttpContext, object> ResponseLogger { get; set; } = (logger, httpContext, response) =>
    {
        if (!logger.IsEnabled(LogLevel.Information))
            return;

        logger.ActionResponse(
            response is ObjectResult result ? result.StatusCode ?? StatusCodes.Status200OK : StatusCodes.Status200OK,
            httpContext.User.GetSubjectId(),
            response is ObjectResult ? response : "Raw Data"
        );
    };
    /// <summary>
    /// Custom handler exception and transform into other response.
    /// </summary>
    /// <remarks>
    /// If we don't process this exception just return (null, null) and the default behavior is executed.
    /// </remarks>
    public Func<ResultExecutingContext, (IActionResult, Exception)>? HandlerException { get; set; }
}
internal static partial class TransformResponseAttributeOptionsLoggerExtensions
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "User: {User}, Response: {@Response}")]
    public static partial void ErrorResponse(this ILogger logger, Exception ex, string user, object response);
    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Code: {Code}, User: {User}, Message: {Message}, Response: {@Response}")]
    public static partial void BadRequestResponse(this ILogger logger, int code, string user, string message, object response);
    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Code: {Code}, User: {User}, Response: {@Response}")]
    public static partial void ActionResponse(this ILogger logger, int code, string user, object response);
}
