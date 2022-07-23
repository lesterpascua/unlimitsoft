using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Linq;

namespace UnlimitSoft.Web.AspNet
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Get ip address for the client.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetIpAddress(this HttpContext context)
        {
            StringValues forwardedForOrProto = StringValues.Empty;
            if (context.Request.Headers?.TryGetValue("x-forwarded-for", out forwardedForOrProto) ?? false)
                return forwardedForOrProto.ToString().Split(',').Select(s => s.Trim()).First();

            if (context.Request.Headers?.TryGetValue("x-real-ip", out forwardedForOrProto) ?? false)
                return forwardedForOrProto.ToString().Split(',').Select(s => s.Trim()).First();
            
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}
