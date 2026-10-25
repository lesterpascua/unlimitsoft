using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIU.Libs.Web;
using System.Globalization;
using System.Net;
using System.Text;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Message;
using UnlimitSoft.Web.AspNet;
using UnlimitSoft.Web.AspNet.Filter;
using UnlimitSoft.Web.Client;

namespace OIU.Libs.AspNet.EndPoint;


/// <summary>
/// 
/// </summary>
public static class MinHelper
{
    /// <summary>
    /// Bind the model from the query string
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public static async ValueTask<T?> BindAsync<T>(HttpContext context)
    {
        var metadataProvider = context.RequestServices.GetRequiredService<IModelMetadataProvider>();
        var modelBinderFactory = context.RequestServices.GetRequiredService<IModelBinderFactory>();
        var modelMetadata = metadataProvider.GetMetadataForType(typeof(T));

        var modelBinder = modelBinderFactory.CreateBinder(new ModelBinderFactoryContext
        {
            BindingInfo = new BindingInfo(),
            Metadata = modelMetadata
        });
        var valueProvider = new QueryStringValueProvider(BindingSource.Query, context.Request.Query, CultureInfo.InvariantCulture);

        var action = new ActionContext { HttpContext = context };
        var bindingContext = DefaultModelBindingContext.CreateBindingContext(action, valueProvider, modelMetadata, null, string.Empty);
        await modelBinder.BindModelAsync(bindingContext);

        if (bindingContext.Result.IsModelSet)
            return (T?)bindingContext.Result.Model;

        return default;
    }

    /// <summary>
    /// Get the result from the Result object and convert it to an IResult
    /// </summary>
    /// <typeparam name="TResult">The type of the result value.</typeparam>
    /// <param name="result">The result object.</param>
    /// <param name="code">The optional HTTP status code.</param>
    /// <returns>An IResult representing the outcome.</returns>
    public static Microsoft.AspNetCore.Http.IResult GetResult<TResult>(in Result<TResult> result, HttpStatusCode? code) => GetResult(result.To(), code ?? HttpStatusCode.OK, null);
    /// <summary>
    /// Get the result from the Result object and convert it to an IResult
    /// </summary>
    /// <param name="result">The result object.</param>
    /// <param name="code">The optional HTTP status code.</param>
    /// <param name="transform"></param>
    /// <returns>An IResult representing the outcome.</returns>
    public static Microsoft.AspNetCore.Http.IResult GetResult(in Result<object> result, HttpStatusCode code, Func<HttpStatusCode, object, ResultCodePairUri>? transform)
    {
        if (result.IsSuccess)
        {
            string? uri = null;
            var value = result.Value!;
            if (transform is not null)
                (code, value, uri) = transform(code, value);

            if (value is OIUFileResult fileResult)
                return Results.File(fileResult.Content, fileResult.ContentType, fileResult.Name);
            return code switch
            {
                HttpStatusCode.Accepted => Results.Accepted(uri, value),
                HttpStatusCode.Created => Results.Created(uri, value),
                HttpStatusCode.NoContent => Results.NoContent(),
                _ or HttpStatusCode.OK => Results.Ok(value)
            };
        }

        var error = result.Error;
        if (error is null)
            return Results.InternalServerError();

        return error.Code switch
        {
            HttpStatusCode.NotFound => Results.NotFound(error.GetErrorBody()),
            HttpStatusCode.BadRequest => Results.BadRequest(error.GetErrorBody()),
            HttpStatusCode.Forbidden => Results.Json(error.GetErrorBody(), statusCode: StatusCodes.Status403Forbidden),
            HttpStatusCode.UnprocessableEntity => Results.UnprocessableEntity(error.GetErrorBody()),
            HttpStatusCode.Conflict => Results.Conflict(error.GetErrorBody()),
            _ => Results.InternalServerError(),
        };
    }
    /// <summary>
    /// Get the result from the Result object and convert it to an IResult
    /// </summary>
    /// <param name="result">The result object.</param>
    /// <param name="code">The optional HTTP status code.</param>
    /// <param name="transform"></param>
    /// <returns>An IResult representing the outcome.</returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> GetResultAsync(Result<object> result, HttpStatusCode code, Func<HttpStatusCode, object, Task<ResultCodePairUri>>? transform)
    {
        if (result.IsSuccess)
        {
            string? uri = null;
            var value = result.Value!;
            if (transform is not null)
                (code, value, uri) = await transform(code, value);

            if (value is OIUFileResult fileResult)
                return Results.File(fileResult.Content, fileResult.ContentType, fileResult.Name);
            return code switch
            {
                HttpStatusCode.Accepted => Results.Accepted(uri, value),
                HttpStatusCode.Created => Results.Created(uri, value),
                HttpStatusCode.NoContent => Results.NoContent(),
                _ or HttpStatusCode.OK => Results.Ok(value)
            };
        }

        var error = result.Error;
        if (error is null)
            return Results.InternalServerError();

        return error.Code switch
        {
            HttpStatusCode.NotFound => Results.NotFound(error.GetErrorBody()),
            HttpStatusCode.BadRequest => Results.BadRequest(error.GetErrorBody()),
            HttpStatusCode.Forbidden => Results.Json(error.GetErrorBody(), statusCode: StatusCodes.Status403Forbidden),
            HttpStatusCode.UnprocessableEntity => Results.UnprocessableEntity(error.GetErrorBody()),
            HttpStatusCode.Conflict => Results.Conflict(error.GetErrorBody()),
            _ => Results.InternalServerError(),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> DispatchQuery<TQuery, TResult>(TQuery request, HttpContext context, CancellationToken ct) where TQuery : IQuery<TResult>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.DispatchAsync(provider, request, ct);
        return GetResult(in result, null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchQuery<TQuery, TResult>(TQuery request, HttpContext context, CancellationToken ct) where TQuery : IQuery<Result<TResult>>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.SafeDispatchAsync(provider, request, ct);
        return GetResult(in result, null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> DispatchCommand<TCommand, TResult>(TCommand request, HttpContext context, CancellationToken ct) where TCommand : ICommand<TResult>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.DispatchAsync(provider, request, ct);
        return GetResult(in result, null);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchCommand<TCommand, TResult>(TCommand request, HttpContext context, HttpStatusCode? code = null, CancellationToken ct = default) where TCommand : ICommand<Result<TResult>>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.SafeDispatchAsync(provider, request, ct);
        return GetResult(in result, code);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> DispatchQueryWithLog<TQuery, TResult>(TQuery request, HttpContext context, CancellationToken ct)
        where TQuery : IQuery<TResult>
    {
        return ActionWithLog(
            context,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

                return dispatcher.DispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> DispatchQueryWithLog<TQuery, TBody, TResult>(TQuery request, TBody? body, HttpContext context, CancellationToken ct)
        where TBody : class
        where TQuery : IQuery<TResult>
    {
        return ActionWithLog(
            context,
            body,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

                return dispatcher.DispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchQueryWithLog<TQuery, TResult>(TQuery request, HttpContext context, CancellationToken ct)
        where TQuery : IQuery<Result<TResult>>
    {
        return ActionWithLog(
            context,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

                return dispatcher.SafeDispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchQueryWithLog<TQuery, TBody, TResult>(TQuery request, TBody? body, HttpContext context, CancellationToken ct)
        where TBody : class
        where TQuery : IQuery<Result<TResult>>
    {
        return ActionWithLog(
            context,
            body,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

                return dispatcher.SafeDispatchAsync(provider, request, ct);
            }
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> DispatchCommandWithLog<TCommand, TResult>(TCommand request, HttpContext context, CancellationToken ct)
        where TCommand : ICommand<TResult>
    {
        return ActionWithLog(
            context,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                return dispatcher.DispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> DispatchCommandWithLog<TCommand, TBody, TResult>(TCommand request, TBody? body, HttpContext context, CancellationToken ct)
        where TBody : class
        where TCommand : ICommand<TResult>
    {
        return ActionWithLog(
            context,
            body,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                return dispatcher.DispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="transform"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> DispatchCommandWithLog<TCommand, TBody, TResult>(TCommand request, TBody? body, HttpContext context, HttpStatusCode code, Func<HttpStatusCode, object, ResultCodePairUri>? transform, CancellationToken ct)
        where TBody : class
        where TCommand : ICommand<TResult>
    {
        return ActionWithLog(
            context,
            body,
            async () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                var aux = await dispatcher.DispatchAsync(provider, request, ct);
                return aux.To();
            },
            code,
            transform
        );
    }
    /// <summary>
    ///
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="transform"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> DispatchCommandWithLogAsync<TCommand, TBody, TResult>(TCommand request, TBody? body, HttpContext context, HttpStatusCode code, Func<HttpStatusCode, object, Task<ResultCodePairUri>>? transform, CancellationToken ct)
        where TBody : class
        where TCommand : ICommand<TResult>
    {
        return ActionWithLogAsync(
            context,
            body,
            async () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                var aux = await dispatcher.DispatchAsync(provider, request, ct);
                return aux.To();
            },
            code,
            transform
        );
    }
    /// <summary>
    ///
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchCommandWithLog<TCommand, TResult>(TCommand request, HttpContext context, CancellationToken ct)
        where TCommand : ICommand<Result<TResult>>
    {
        return ActionWithLog(
            context,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                return dispatcher.SafeDispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchCommandWithLog<TCommand, TBody, TResult>(TCommand request, TBody? body, HttpContext context, CancellationToken ct)
        where TBody : class
        where TCommand : ICommand<Result<TResult>>
    {
        return ActionWithLog(
            context,
            body,
            () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                return dispatcher.SafeDispatchAsync(provider, request, ct);
            }
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="transform"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchCommandWithLog<TCommand, TBody, TResult>(TCommand request, TBody? body, HttpContext context, HttpStatusCode code, Func<HttpStatusCode, object, ResultCodePairUri>? transform, CancellationToken ct)
        where TBody : class
        where TCommand : ICommand<Result<TResult>>
    {
        return ActionWithLog(
            context,
            body,
            async () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                var aux = await dispatcher.SafeDispatchAsync(provider, request, ct);
                return aux.To();
            },
            code,
            transform
        );
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="body"></param>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="transform"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchCommandWithLogAsync<TCommand, TBody, TResult>(TCommand request, TBody? body, HttpContext context, HttpStatusCode code, Func<HttpStatusCode, object, Task<ResultCodePairUri>>? transform, CancellationToken ct)
        where TBody : class
        where TCommand : ICommand<Result<TResult>>
    {
        return ActionWithLogAsync(
            context,
            body,
            async () =>
            {
                var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
                var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

                var aux = await dispatcher.SafeDispatchAsync(provider, request, ct);
                return aux.To();
            },
            code,
            transform
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TBody"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="context"></param>
    /// <param name="body"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> ActionWithLog<TBody, TResult>(HttpContext context, TBody? body, Func<ValueTask<Result<TResult>>> action)
        where TBody : class
    {
        return await ActionWithLog(context, body, async () => (await action()).To(), HttpStatusCode.OK, null);
    }
    /// <summary>
    /// Executes the specified action, logging the HTTP request and response details, and returns the result as an HTTP
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the action.</typeparam>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
    /// <param name="action">A delegate representing the asynchronous action to execute. The action should return a result of type TResult
    /// wrapped in a message result.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an HTTP result representing the
    /// outcome of the action.</returns>
    public static Task<Microsoft.AspNetCore.Http.IResult> ActionWithLog<TResult>(HttpContext context, Func<ValueTask<Result<TResult>>> action) => ActionWithLog<object, TResult>(context, null, action);
    /// <summary>
    /// Executes the specified action, logging the HTTP request and response details, and returns the result as an HTTP
    /// response.
    /// </summary>
    /// <typeparam name="TBody">The type of the request body to be processed. Must be a reference type.</typeparam>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
    /// <param name="body">The request body to be passed to the action. Can be null if the action does not require a body.</param>
    /// <param name="action">A delegate representing the asynchronous action to execute. The action should return a result of type TResult
    /// wrapped in a message result.</param>
    /// <param name="code"></param>
    /// <param name="transform"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an HTTP result representing the
    /// outcome of the action.</returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> ActionWithLog<TBody>(HttpContext context, TBody? body, Func<ValueTask<Result<object>>> action, HttpStatusCode code, Func<HttpStatusCode, object, ResultCodePairUri>? transform)
        where TBody : class
    {
        const string
            REQUEST_MESSAGE = "API Request {TraceId} from {Ip}, {Method} {Url}, Body = {@Body}, Header = {Header}",
            RESPONSE_MESSAGE = "API Response {TraceId} Code = {StatusCode}, Body = {@Response}";

        var req = context.Request;
        var logger = context.RequestServices.GetRequiredService<ILogger<IEndPointRegister>>();
        var options = context.RequestServices.GetService<IOptions<RequestLoggerAttribute.Options>>()?.Value;

        var level = options?.LogLevel ?? LogLevel.Information;
        if (!logger.IsEnabled(level))
        {
            var aux = await action();
            return GetResult(aux, code, transform);
        }

        // Log request and response in the same log entry, so we can easily correlate them using TraceId
        string? error = null;
        Result<object> result = default!;
        var correlationId = context.TraceIdentifier;
        logger.Log(level, REQUEST_MESSAGE, correlationId, context.GetIpAddress(), req.Method, req.GetDisplayUrl(), body, GetHeader(req.Headers));
        try
        {
            result = await action();
        }
        catch (Exception ex)
        {
            error = $$"""{"error":"{{ex.Message}}"}""";
            throw;
        }
        finally
        {
            if (error is null)
            {
                var statusCode = result.Error?.Code ?? HttpStatusCode.OK;
                var value = result.IsSuccess ? result.Value : result.Error?.GetBody();

                logger.Log(level, RESPONSE_MESSAGE, correlationId, (int)statusCode, value);
            }
            else
                logger.Log(level, RESPONSE_MESSAGE, correlationId, (int)HttpStatusCode.InternalServerError, error);
        }

        return GetResult(result, code, transform);
    }
    /// <summary>
    /// Executes the specified action, logging the HTTP request and response details, and returns the result as an HTTP
    /// response.
    /// </summary>
    /// <typeparam name="TBody">The type of the request body to be processed. Must be a reference type.</typeparam>
    /// <param name="context">The HTTP context for the current request. Provides access to request and response information.</param>
    /// <param name="body">The request body to be passed to the action. Can be null if the action does not require a body.</param>
    /// <param name="action">A delegate representing the asynchronous action to execute. The action should return a result of type TResult
    /// wrapped in a message result.</param>
    /// <param name="code"></param>
    /// <param name="transform"></param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an HTTP result representing the
    /// outcome of the action.</returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> ActionWithLogAsync<TBody>(HttpContext context, TBody? body, Func<ValueTask<Result<object>>> action, HttpStatusCode code, Func<HttpStatusCode, object, Task<ResultCodePairUri>>? transform)
        where TBody : class
    {
        const string
            REQUEST_MESSAGE = "API Request {TraceId} from {Ip}, {Method} {Url}, Body = {@Body}, Header = {Header}",
            RESPONSE_MESSAGE = "API Response {TraceId} Code = {StatusCode}, Body = {@Response}";

        var req = context.Request;
        var logger = context.RequestServices.GetRequiredService<ILogger<IEndPointRegister>>();
        var options = context.RequestServices.GetService<IOptions<RequestLoggerAttribute.Options>>()?.Value;

        var level = options?.LogLevel ?? LogLevel.Information;
        if (!logger.IsEnabled(level))
        {
            var aux = await action();
            return await GetResultAsync(aux, code, transform);
        }

        // Log request and response in the same log entry, so we can easily correlate them using TraceId
        string? error = null;
        Result<object> result = default!;
        var correlationId = context.TraceIdentifier;
        logger.Log(level, REQUEST_MESSAGE, correlationId, context.GetIpAddress(), req.Method, req.GetDisplayUrl(), body, GetHeader(req.Headers));
        try
        {
            result = await action();
        }
        catch (Exception ex)
        {
            error = $$"""{"error":"{{ex.Message}}"}""";
            throw;
        }
        finally
        {
            if (error is null)
            {
                var statusCode = result.Error?.Code ?? HttpStatusCode.OK;
                var value = result.IsSuccess ? result.Value : result.Error?.GetBody();

                logger.Log(level, RESPONSE_MESSAGE, correlationId, (int)statusCode, value);
            }
            else
                logger.Log(level, RESPONSE_MESSAGE, correlationId, (int)HttpStatusCode.InternalServerError, error);
        }

        return await GetResultAsync(result, code, transform);
    }

    #region Private Methods
    /// <summary>
    /// Headers holding credentials (session cookie, bearer token, antiforgery token), their values are never logged.
    /// </summary>
    private static readonly HashSet<string> _redactedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Cookie",
        "Authorization",
        "Proxy-Authorization",
        "X-XSRF-TOKEN"
    };

    private static string GetHeader(IHeaderDictionary headers)
    {
        if (headers.Count == 0)
            return "[]";

        const string END_VALUE = "] }, ";
        var sb = new StringBuilder().Append('[');
        foreach (var header in headers)
        {
            sb.Append("{ Key: \"").Append(header.Key).Append("\", Value: [");
            if (header.Value.Count == 0)
            {
                sb.Append(END_VALUE);
                continue;
            }

            var redacted = _redactedHeaders.Contains(header.Key);
            foreach (var value in header.Value)
                sb.Append('"').Append(redacted ? "***" : value).Append("\", ");
            sb.Remove(sb.Length - 2, 2).Append(END_VALUE);
        }
        return sb.Remove(sb.Length - 2, 2).Append(']').ToString();
    }
    #endregion
}
