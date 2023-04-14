using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Linq;
using UnlimitSoft.Message;

namespace UnlimitSoft.Web.AspNet;


/// <summary>
/// 
/// </summary>
public static class HttpContextExtensions
{
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
    /// <returns></returns>
    public static string? GetIpAddress(this HttpContext context)
    {
        StringValues forwardedForOrProto = StringValues.Empty;
        if (context.Request.Headers?.TryGetValue(HeaderXForwardedFor, out forwardedForOrProto) ?? false)
            return forwardedForOrProto.ToString().Split(',').Select(s => s.Trim()).First();

        if (context.Request.Headers?.TryGetValue(HeaderXRealIp, out forwardedForOrProto) ?? false)
            return forwardedForOrProto.ToString().Split(',').Select(s => s.Trim()).First();
        
        return context.Connection.RemoteIpAddress?.ToString();
    }
    /// <summary>
    /// Convert result into action result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="controller"></param>
    /// <returns></returns>
    public static ActionResult<T> ToActionResult<T>(this in Result<T> result, ControllerBase controller)
    {
        if (result.Error is null)
            return controller.Ok(result.Value);
        return controller.StatusCode((int)result.Error.Code, result.Error.GetBody());
    }
}
