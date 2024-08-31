using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.AspNet;


/// <summary>
/// 
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Unknow ip name
    /// </summary>
    public const string Unknow = "unknow";
    /// <summary>
    /// Header where the real ip is store when is forwarding using proxy
    /// </summary>
    public const string HeaderXRealIp = "x-real-ip";
    /// <summary>
    /// 
    /// </summary>
    public const string HeaderXForwardedFor = "x-forwarded-for";

    /// <summary>
    /// Get ip address for the client.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>Ip where the request was made, <see cref="Unknow"/> if the ip can't be resolved.</returns>
    public static string GetIpAddress(this HttpContext context)
    {
        StringValues forwardedForOrProto = StringValues.Empty;
        if (context.Request.Headers?.TryGetValue(HeaderXForwardedFor, out forwardedForOrProto) ?? false)
            return forwardedForOrProto.ToString().Split(',', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

        if (context.Request.Headers?.TryGetValue(HeaderXRealIp, out forwardedForOrProto) ?? false)
            return forwardedForOrProto.ToString().Split(',', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim();

        return context.Connection.RemoteIpAddress?.ToString() ?? Unknow;
    }
    /// <summary>
    /// Convert result into action result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="controller"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static ActionResult<T> ToActionResult<T>(this in Result<T> result, ControllerBase controller, HttpStatusCode code = HttpStatusCode.OK)
    {
        if (result.Error is null)
            return controller.StatusCode((int)code, result.Value);
        return controller.StatusCode((int)result.Error.Code, result.Error.GetBody());
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="value"></param>
    /// <param name="controller"></param>
    /// <param name="transform"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static ActionResult<TOut> ToActionResult<TIn, TOut>(this in Result<TIn> value, ControllerBase controller, Func<TIn, TOut> transform, HttpStatusCode code = HttpStatusCode.OK)
    {
        if (!value.IsSuccess)
            return Result.FromError<TOut>(value.Error!).ToActionResult(controller, code);

        var v = transform(value.Value!);
        return Result.FromOk(v).ToActionResult(controller, code);
    }
}
