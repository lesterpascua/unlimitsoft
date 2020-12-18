using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoftUnlimit.Web.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.AspNet.ErrorHandling
{
    /// <summary>
    /// Transform exception to respond
    /// </summary>
    public class ToResponseExceptionHandlerOptions : ExceptionHandlerOptions
    {
        private readonly bool _showExceptionInfo;
        private readonly IEnumerable<IExceptionHandler> _handlers;
        private readonly ILogger<ToResponseExceptionHandlerOptions> _logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="showExceptionInfo"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        public ToResponseExceptionHandlerOptions(
            bool showExceptionInfo = true,
            IEnumerable<IExceptionHandler> handlers = null, 
            ILogger<ToResponseExceptionHandlerOptions> logger = null)
        {
            _showExceptionInfo = showExceptionInfo;
            _logger = logger;
            _handlers = handlers;
            ExceptionHandler = (context) => ExceptionHandlerInternal(context, _showExceptionInfo, _handlers, _logger);
        }

        /// <summary>
        /// Write exception
        /// </summary>
        /// <param name="context"></param>
        /// <param name="showExceptionInfo"></param>
        /// <param name="handlers"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static async Task ExceptionHandlerInternal(HttpContext context, bool showExceptionInfo, IEnumerable<IExceptionHandler> handlers = null, ILogger<ToResponseExceptionHandlerOptions> logger = null)
        {
            var feature = context.Features.Get<IExceptionHandlerFeature>();
            var jsonHelper = context.RequestServices.GetService<IJsonHelper>();

            logger?.LogError(feature.Error, $"User: {context.User.Identity.Name}, logged in from: {context.GetIpAddress()}");
            if (handlers != null)
                foreach (var handler in handlers.Where(x => x.ShouldHandle(context)))
                    await handler.HandleAsync(context);

            var reason = showExceptionInfo ? feature.Error : new Exception("Consult admin for more information.");
            var response = new Response<IDictionary<string, Exception>> {
                IsSuccess = false,
                Code = StatusCodes.Status500InternalServerError,
                Body = new Dictionary<string, Exception> {
                    { string.Empty, reason }
                },
                UIText = "Server Error"
            };

            context.Response.StatusCode = response.Code;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonHelper.Serialize(response).ToString());
        }
    }
}
