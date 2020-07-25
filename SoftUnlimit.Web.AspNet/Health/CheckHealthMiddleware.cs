using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Health
{
    /// <summary>
    /// 
    /// </summary>
    public class CheckHealthMiddleware
    {
        private readonly string _infoText;
        private readonly string _retryAfter;
        private readonly RequestDelegate _next;
        private readonly CheckHealthContext _context;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="context"></param>
        /// <param name="retryAfter">Retry time in seconds</param>
        /// <param name="infoText"></param>
        public CheckHealthMiddleware(
            RequestDelegate next, 
            CheckHealthContext context, 
            int retryAfter, 
            string infoText = "Service Unavailable")
        {
            this._next = next;
            this._context = context;
            this._infoText = infoText;
            this._retryAfter = retryAfter.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            if (_context.Healthy == HealthStatus.Unhealthy)
            {
                var response = httpContext.Response;

                response.Headers["Retry-After"] = this._retryAfter;
                response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await response.WriteAsync(this._infoText);
            } else
                await _next(httpContext);
        }
    }
}
