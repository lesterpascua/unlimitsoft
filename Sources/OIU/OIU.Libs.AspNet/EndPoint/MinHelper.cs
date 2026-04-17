using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using OIU.Libs.Domain;
using OIU.Libs.Web;
using System.Globalization;
using System.Net;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Message;
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
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> DispatchQuery<TQuery, TResult>(TQuery request, HttpContext context, CancellationToken ct) where TQuery : OIUQuery<TResult>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.DispatchAsync(provider, request, ct);
        return GetResult(in result);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchQuery<TQuery, TResult>(TQuery request, HttpContext context, CancellationToken ct) where TQuery : OIUQuery<Result<TResult>>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<IQueryDispatcher>();

        var result = await dispatcher.SafeDispatchAsync(provider, request, ct);
        return GetResult(in result);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> DispatchCommand<TCommand, TResult>(TCommand request, HttpContext context, CancellationToken ct) where TCommand : OIUCommand<TResult>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.DispatchAsync(provider, request, ct);
        return GetResult(in result);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <param name="code"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<Microsoft.AspNetCore.Http.IResult> SafeDispatchCommand<TCommand, TResult>(TCommand request, HttpContext context, HttpStatusCode? code = null, CancellationToken ct = default) where TCommand : OIUCommand<Result<TResult>>
    {
        var provider = context.RequestServices.GetRequiredService<IServiceProvider>();
        var dispatcher = context.RequestServices.GetRequiredService<ICommandDispatcher>();

        var result = await dispatcher.SafeDispatchAsync(provider, request, ct);
        return GetResult(in result, code);
    }

    #region Private Methods
    private static Microsoft.AspNetCore.Http.IResult GetResult<TResult>(in Result<TResult> result, HttpStatusCode? code = null)
    {
        if (result.IsSuccess)
        {
            var value = result.Value!;
            if (value is OIUFileResult fileResult)
                return Results.File(fileResult.Content, fileResult.ContentType, fileResult.Name);
            return code switch
            {
                HttpStatusCode.Accepted => Results.Accepted(string.Empty, value),
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
            HttpStatusCode.UnprocessableEntity => Results.UnprocessableEntity(error.GetErrorBody()),
            HttpStatusCode.Conflict => Results.Conflict(error.GetErrorBody()),
            _ => Results.InternalServerError(),
        };
    }
    #endregion
}
