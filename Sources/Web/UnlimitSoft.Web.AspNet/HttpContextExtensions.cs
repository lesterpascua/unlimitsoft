using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
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
    /// Header where the client-ip is store when is forwarding using proxy
    /// </summary>
    public const string HeaderXClientIP = "client-ip";
    /// <summary>
    /// 
    /// </summary>
    public const string HeaderXForwardedFor = "x-forwarded-for";

    /// <summary>
    /// Get ip address for the client.
    /// </summary>
    /// <param name="this"></param>
    /// <param name="allowHeader">Allow get if from the headers</param>
    /// <returns>Ip where the request was made, <see cref="Unknow"/> if the ip can't be resolved.</returns>
    public static string GetIpAddress(this HttpContext @this, AllowedHeader allowHeader = AllowedHeader.XForwardedFor)
    {
        StringValues forwardedForOrProto;

        if (allowHeader.HasFlag(AllowedHeader.XForwardedFor) && @this.Request.Headers.TryGetValue(HeaderXForwardedFor, out forwardedForOrProto))
            return CleanIPAddress(forwardedForOrProto.ToString().Split(',', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim());

        if (allowHeader.HasFlag(AllowedHeader.XRealIP) && @this.Request.Headers.TryGetValue(HeaderXRealIp, out forwardedForOrProto))
            return CleanIPAddress(forwardedForOrProto.ToString().Split(',', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim());

        if (allowHeader.HasFlag(AllowedHeader.ClientIP) && @this.Request.Headers.TryGetValue(HeaderXClientIP, out forwardedForOrProto))
            return CleanIPAddress(forwardedForOrProto.ToString().Split(',', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim());

        return @this.Connection.RemoteIpAddress?.ToString() ?? Unknow;
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
            return Result.Err<TOut>(value.Error!).ToActionResult(controller, code);

        var v = transform(value.Value!);
        return Result.Ok(v).ToActionResult(controller, code);
    }

    #region Private Methods
    private static string CleanIPAddress(string ipAddress)
    {
        var span = ipAddress.AsSpan();

        var update = false;
        var end = span.IndexOf(']');
        if (end != -1 && span[0] == '[')
        {
            update = true;
            span = span[1..end];
        }

        var count = 0;
        for (var i = 0; i < span.Length; i++)
            if (span[i] == ':' && ++count > 1)
                break;
        if (count == 1)
        {
            update = true;
            span = span[..span.LastIndexOf(':')];
        }

        if (update)
            return span.ToString();
        return ipAddress;
    }
    #endregion

    /// <summary>
    /// Headers allowed to get the ip address
    /// </summary>
    [Flags]
    public enum AllowedHeader
    {
        /// <summary>
        /// None header allowed
        /// </summary>
        None = 0,
        /// <summary>
        /// 
        /// </summary>
        XForwardedFor = 1,
        /// <summary>
        /// 
        /// </summary>
        XRealIP = 2,
        /// <summary>
        /// 
        /// </summary>
        ClientIP = 4,
        /// <summary>
        /// Cheall all availables headers
        /// </summary>
        All = XForwardedFor | XRealIP | ClientIP
    }
}
