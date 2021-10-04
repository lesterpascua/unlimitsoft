using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.Health
{
    /// <summary>
    /// 
    /// </summary>
    public class CheckHealthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CheckHealthContext _context;
        private readonly CheckHealthMiddlewareOptions _options;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
        public CheckHealthMiddleware(RequestDelegate next, CheckHealthContext context, CheckHealthMiddlewareOptions options)
        {
            this._next = next;
            this._context = context;
            this._options = options;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            string path = httpContext.Request.Path;
            if (this._options.SkipUrls?.Contains(path) != true && this._context.Healthy == HealthStatus.Unhealthy)
            {
                var response = httpContext.Response;

                response.Headers["Retry-After"] = this._options.RetryAfter.ToString();
                response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await response.WriteAsync(this._options.InfoText);
            } else
                await _next(httpContext);
        }
    }
}
