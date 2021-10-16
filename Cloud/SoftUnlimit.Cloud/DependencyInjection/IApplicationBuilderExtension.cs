using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SoftUnlimit.Cloud.Security;
using SoftUnlimit.Web.AspNet.ErrorHandling;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SoftUnlimit.Cloud.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class IApplicationBuilderExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="showExceptionDetails"></param>
        ///// <param name="useSerilogRequestLogging"></param>
        /// <param name="errorBody">Allow get a customer error depending of the error context.</param>
        /// <param name="handlers"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWrapperDevelopment(this IApplicationBuilder app,
            bool showExceptionDetails,
            //bool useSerilogRequestLogging = false,
            Func<HttpContext, Dictionary<string, string[]>> errorBody = null,
            params IExceptionHandler[] handlers)
        {
            var handlerException = new ToResponseExceptionHandlerOptions(showExceptionDetails, handlers: handlers, errorBody: errorBody);
            app.UseExceptionHandler(handlerException);
            //if (useSerilogRequestLogging)
            //    app.UseSerilogRequestLogging();

            return app;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="title"></param>
        /// <param name="version"></param>
        /// <param name="apiPrefix"></param>
        /// <param name="protect">If true add a middleware to deny access to the swagger UI if the apikey not math with the one set in the system. The parameter is expecting as querystring.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseWrapperSwagger(this IApplicationBuilder app, string title, string version, string apiPrefix, bool protect = true)
        {
            if (protect)
                app.UseMiddleware<SwaggerInterceptor>();

            app.UseSwagger(c =>
            {
                //c.RouteTemplate
                c.PreSerializeFilters.Add((doc, request) =>
                {
                    if (!string.IsNullOrWhiteSpace(apiPrefix))
                    {
                        var paths = doc.Paths
                            .Select(s => (Key: apiPrefix + s.Key, s.Value))
                            .ToArray();

                        doc.Paths.Clear();
                        foreach (var (Key, Value) in paths)
                            doc.Paths.Add(Key, Value);
                    }
                });
            });
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(DocExpansion.None);
                c.SwaggerEndpoint($"{version}/swagger.json", title);
            });

            return app;
        }

        #region Nested Classes
        private sealed class SwaggerInterceptor
        {
            private readonly RequestDelegate _next;
            private readonly string _apiKey;


            public SwaggerInterceptor(RequestDelegate next, IOptions<AuthorizeOptions> authorize)
            {
                _next = next;
                _apiKey = authorize.Value.ApiKey;
            }

            public async Task Invoke(HttpContext context)
            {
                var uri = context.Request.Path.ToString();
                if (uri.Contains("/swagger/index.html") || uri.Contains("/swagger/v1/swagger.json"))
                {
                    var param = HttpUtility
                        .ParseQueryString(context.Request.QueryString.Value);

                    var apikey = param["apikey"];
                    if (!string.IsNullOrEmpty(apikey))
                    {
                        try
                        {
                            apikey = Encoding.UTF8.GetString(Convert.FromBase64String(apikey));
                        }
                        catch
                        {
                            apikey = null;
                        }
                    }

                    if (apikey != _apiKey)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.ContentType = "application/json";
                        return;
                    }
                }

                await _next.Invoke(context);
            }
        }
        #endregion
    }
}
