using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OIU.Libs.AspNet.Configuration;
using OIU.Libs.AspNet.EndPoint;
using OIU.Libs.AspNet.Logging;
using OIU.Libs.Configuration;
using OIU.Libs.Security;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using UnlimitSoft.Data;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.AspNet;
using UnlimitSoft.Message;
using UnlimitSoft.Web.AspNet.Filter;

namespace OIU.Libs.AspNet.DependencyInjection;


/// <summary>
/// 
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="serializer"></param>
    /// <param name="options"></param>
    /// <param name="showExceptionDetails"></param>
    /// <param name="compilation"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    /// <returns></returns>
    public static IApplicationBuilder StartApp(this IApplicationBuilder app, IJsonSerializer serializer, ServiceOptions options, bool showExceptionDetails, string compilation, ILogger logger, ExceptionHandlerOptions? exceptionHandler = null)
    {
        var service = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var correlationTrusted = app.ApplicationServices.GetRequiredService<ICorrelationTrusted>();
        var exceptionHandlerLogger = app.ApplicationServices.GetRequiredService<ILogger<OIUToResponseExceptionHandlerOptions>>();

        logger.LogInformation("Starting, ENV: {Environment}, COMPILER: {Compilation}, Region: {Region} ...", service.EnvironmentName, compilation, options.Region);

        app.UseMiddleware<OIULoggerMiddleware>();
        exceptionHandler ??= new OIUToResponseExceptionHandlerOptions(
            serializer,
            correlationTrusted,
            showExceptionDetails,
            handlers: null,
            errorBody: null,
            logger: exceptionHandlerLogger
        );
        app.UseExceptionHandler(exceptionHandler);
        return app;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    /// <param name="options"></param>
    /// <param name="version"></param>
    /// <param name="variants">variant aceptted by the api (v1, v2) implied 2 variant.</param>
    /// <param name="prefix">Prefix asociate to the route in swagger the final path is prefix + path</param>
    /// <param name="restrictAccess">If true add a middleware to deny access to the swagger UI if the apikey not math with the one set in the system. The parameter is expecting as querystring.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseDocumentation(this IApplicationBuilder app, DocumentationOptions options, Version version, int[] variants, string? prefix, bool restrictAccess)
    {
        if (options.Disable)
            return app;

        if (restrictAccess)
            app.UseMiddleware<SwaggerInterceptor>();

        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((doc, request) =>
            {
                var index = doc.Info.Version!.IndexOf('-');
                if (index == -1)
                    return;
                var apiVersion = doc.Info.Version[..index];
                var v = int.Parse(apiVersion);

                var apiStartWith = $"/api/v{v}/";
                var paths = doc.Paths
                    .Where(x => x.Key.StartsWith(apiStartWith))
                    .Select(s =>
                    {
                        var path = s.Key;
                        if (!string.IsNullOrEmpty(prefix))
                            path = $"{prefix}{path}";

                        return (Key: path, s.Value);
                    }).ToArray();

                doc.Paths.Clear();
                foreach (var (Key, Value) in paths)
                    doc.Paths.Add(Key, Value);
            });
        });
        app.UseSwaggerUI(c =>
        {
            c.DocExpansion(DocExpansion.None);
            for (var i = 0; i < variants.Length; i++)
            {
                var apiVersion = $"{variants[i]}-{version}";
                c.SwaggerEndpoint($"{apiVersion}/swagger.json", $"API v{apiVersion}");
            }
        });

        return app;
    }
    /// <summary>
    /// Register all the endpoints in the assemblies for minimal api
    /// </summary>
    /// <param name="app"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseEndpointRegistration(this WebApplication app, Assembly[] assemblies)
    {
        var endpointTypes = assemblies
            .SelectMany(a => a.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterface(nameof(IEndPointRegister)) is not null));
        foreach (var type in endpointTypes)
        {
            var endpoint = (IEndPointRegister)ActivatorUtilities.CreateInstance(
                app.Services, 
                type
            );
            endpoint.Register(app);
        }
        return app;
    }

    /// <summary>
    /// Configure the service and start the run
    /// </summary>
    /// <param name="app"></param>
    /// <param name="compilation"></param>
    /// <param name="options"></param>
    /// <param name="exceptionHandler"></param>
    /// <returns></returns>
    internal static async Task OIUConfigureAsync<TServiceOptions>(this WebApplication app, string compilation, OIUServicesConfiguration options, Func<IServiceProvider, ExceptionHandlerOptions>? exceptionHandler = null)
        where TServiceOptions : ServiceOptions
    {
        var serviceOptions = app.Services.GetRequiredService<IOptions<TServiceOptions>>().Value!;
        var documentationOptions = app.Services.GetRequiredService<IOptions<DocumentationOptions>>().Value!;
        var transformOptions = app.Services.GetRequiredService<IOptions<TransformResponseAttributeOptions>>().Value!;

        if (options.OnInit.BeforeInit is not null)
            await options.OnInit.BeforeInit(app);

        var environment = app.Environment;
        var configuration = app.Configuration;
        var serializer = app.Services.GetRequiredService<IJsonSerializer>();
        var factory = app.Services.GetRequiredService<IServiceScopeFactory>();
        //var eventBusOption = app.Services.GetService<IOptions<LocalAzureEventBusOptions>>();

        var showExceptionDetails = environment.IsDevelopment() || transformOptions.ShowExceptionDetails;
        app.StartApp(serializer, serviceOptions, showExceptionDetails, compilation, app.Logger, exceptionHandler?.Invoke(app.Services));

        if (options.OnInit.BeforeMigration is not null)
            await options.OnInit.BeforeMigration(app);

        //var eventBus = eventBusOption is not null && eventBusOption.Value.IsPublisher();
        await UnlimitSoft.Data.EntityFramework.Utility.InitHelper.InitAsync(
            factory,
            options.Database.Enabled ? typeof(IUnitOfWork) : null,
            seedAssemblies: GetDbSeed(options.Database),
            //eventBus: eventBus,
            //eventListener: eventBusOption?.Value.ListenQueues?.Any(p => p.Active == true) ?? false,
            //publishWorker: eventBus,
            logger: app.Logger,
            loadEvent: !serviceOptions.IsSlave,
            eventBashSize: 10_000,
            setup: options.OnInit.AfterMigration is not null ? (_) => options.OnInit.AfterMigration(app) : null
        );
        app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

        if (!environment.IsDevelopment())
            app.UseHsts();
        app.UseHttpsRedirection();

        if (options.AspNet.CorsOrigin is not null && options.AspNet.CorsOrigin.Length != 0)
            app.UseCors(options.AspNet.CorsPolicy);

        await AuthenticationAsync(options, app);

        app.UseOutputCache();

        if (documentationOptions is not null)
            app.UseDocumentation(documentationOptions, options.Documentation.Version, variants: options.Documentation.Variants, documentationOptions.Prefix, options.Documentation.RestrictAccess);

        app.UseHealthChecks("/hc", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        await RoutesAsync(options, app);

        if (options.OnInit.BeforeRun is not null)
            await options.OnInit.BeforeRun(app);

        app.Run();

        // ==============================================================================================================================
        static Task AuthenticationAsync(OIUServicesConfiguration options, WebApplication app)
        {
            if (options.OnInit.OverrideAuth is not null)
                return options.OnInit.OverrideAuth(app);

            app.UseAuthentication();
            app.UseAuthorization();
            return Task.CompletedTask;
        }
        static Task RoutesAsync(OIUServicesConfiguration options, WebApplication app)
        {
            if (options.OnInit.OverrideRoutes is not null)
                return options.OnInit.OverrideRoutes(app);

            app.UseEndpointRegistration(options.AspNet.Endpoints);
            if (options.AspNet.EndpointType != EndpointType.OnlyEndpoints)
                app.MapControllers().RequireAuthorization();
            return Task.CompletedTask;
        }
        static Assembly[]? GetDbSeed(OIUServicesConfiguration.DatabaseSection databaseSection)
        {
            if (!databaseSection.Enabled || databaseSection.DbWriteContext is null)
                return null;

            return databaseSection.Seeds ?? [databaseSection.DbWriteContext.Assembly];
        }
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
                var qs = context.Request.QueryString.Value;
                if (qs is null)
                    return;

                var param = HttpUtility.ParseQueryString(qs);
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
    private sealed class OIUToResponseExceptionHandlerOptions : UnlimitSoft.Web.AspNet.ErrorHandling.ToResponseExceptionHandlerOptions
    {
        private readonly ICorrelationTrusted _trusted;
        private readonly bool _showExceptionInfo;
        private readonly Dictionary<string, string[]> _defaultErrorBody;
        private readonly Func<HttpContext, Dictionary<string, string[]>>? _errorBody;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="trusted"></param>
        /// <param name="showExceptionInfo"></param>
        /// <param name="level">Level of the log.</param>
        /// <param name="handlers"></param>
        /// <param name="fatalErrorCode"></param>
        /// <param name="errorBody"></param>
        /// <param name="contentType">Content type in the response by default application/json</param>
        /// <param name="logger"></param>
        public OIUToResponseExceptionHandlerOptions(
            IJsonSerializer serializer,
            ICorrelationTrusted trusted,
            bool showExceptionInfo = true,
            LogLevel level = LogLevel.Information,
            IEnumerable<UnlimitSoft.Web.AspNet.ErrorHandling.IExceptionHandler>? handlers = null,
            int fatalErrorCode = -1,
            Func<HttpContext, Dictionary<string, string[]>>? errorBody = null,
            string contentType = "application/json",
            ILogger<OIUToResponseExceptionHandlerOptions>? logger = null
        ) : base(serializer, showExceptionInfo, level, handlers, fatalErrorCode, errorBody, contentType, logger)
        {
            _trusted = trusted;
            _showExceptionInfo = showExceptionInfo;
            _errorBody = errorBody;
            _defaultErrorBody = new()
            {
                [string.Empty] = [fatalErrorCode.ToString()]
            };
        }

        /// <inheritdoc />
        protected override StringValues GetCorrelation(HttpContext context)
        {
            var isTrusted = _trusted is null || _trusted.IsTrustedRequest(context);
            if (isTrusted && context.Request.Headers.TryGetValue(UnlimitSoft.Constants.HeaderCorrelation, out var correlationFromHeader))
                return correlationFromHeader;
            return context.TraceIdentifier;
        }
        /// <inheritdoc />
        protected override object GetResponse(HttpContext context, IExceptionHandlerFeature feature, out HttpStatusCode code)
        {
            const string v3 = "/api/v3/";

            if (feature.Error is ResponseException exception)
            {
                code = exception.Code;
                if (context.Request.Path.Value?.StartsWith(v3) == true)
                    return exception.Body;
                return new Response<IDictionary<string, string[]>>(exception.Code, exception.Body);
            }

            object body = feature.Error;
            code = HttpStatusCode.InternalServerError;
            if (!_showExceptionInfo)
                body = _errorBody?.Invoke(context) ?? _defaultErrorBody;

            if (context.Request.Path.Value?.StartsWith(v3) == true)
                return body;
            return new Response<object>(code, body);
        }
    }
    #endregion
}
