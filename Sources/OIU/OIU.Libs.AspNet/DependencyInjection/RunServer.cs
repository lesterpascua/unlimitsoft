using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIU.Libs.AspNet.Configuration;
using OIU.Libs.AspNet.Logging;
using OIU.Libs.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Text.Json;
using UnlimitSoft.Logger;

namespace OIU.Libs.AspNet.DependencyInjection;


/// <summary>
/// Initialize service
/// </summary>
public static class RunServer
{
    /// <summary>
    /// Start service
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    /// <typeparam name="TServiceOptions"></typeparam>
    /// <param name="args">Initialization args</param>
    /// <param name="compilation">Service compilation: RELEASE, DEBUG, etc</param>
    /// <param name="webAppType">Allow register an smaller version of the application is only support after net 8</param>
    /// <param name="setup">Configure onejn configuration service</param>
    /// <param name="configureHostBuilder">Allow configure the host builder just before add serilog middleware</param>
    /// <param name="initSerializer">Initialize the serializer</param>
    /// <param name="exceptionHandler">Indicate the error handler is not the default and will be replace by app.UseExceptionHandler("/error");</param>
    /// <returns></returns>
    public static async Task RunAsync<TStartup, TServiceOptions>(
        string[] args,
        string compilation,
        WebApplicationType webAppType,
        Action<WebApplicationBuilder, OIUServicesConfiguration> setup,
        Func<IHostBuilder, IServiceCollection, ILoggerFactory, IHostBuilder> configureHostBuilder,
        Action<JsonSerializerOptions, JsonSerializerOptions>? initSerializer = null,
        Func<IServiceProvider, ExceptionHandlerOptions>? exceptionHandler = null
    )
        where TServiceOptions : ServiceOptions
    {
        var loggerFactory = new MonitoringSerilogLoggerFactory((Serilog.ILogger?)EnvTypeHelper.Logger ?? CreateConsoleLogger());
        var logger = loggerFactory.CreateLogger<TStartup>();

        try
        {
            InitHelper.Init(initSerializer);

            var builder = webAppType switch
            {
                WebApplicationType.Full => WebApplication.CreateBuilder(args),
                WebApplicationType.Slim => WebApplication.CreateSlimBuilder(args),
                WebApplicationType.Empty or _ => throw new NotSupportedException("Empty is not supported in this version")
            };

            OIUServicesConfiguration? config = null;
            configureHostBuilder(builder.Host, builder.Services, loggerFactory).UseSerilog((context, provider, loggerConfiguration) =>
            {
                if (config is null)
                    throw new InvalidOperationException("Config need to be initialized at this point");

                var @override = provider.GetService<IOverrideLogger>();
                if (@override is not null)
                {
                    @override.Configure(loggerConfiguration);
                    return;
                }

                var environment = provider.GetRequiredService<IHostEnvironment>();
                var loggerOptions = provider.GetRequiredService<IOptions<LoggerOptions>>().Value;
                var serviceOptions = provider.GetRequiredService<IOptions<TServiceOptions>>().Value;

                var env = EnvType.LOCAL;
                if (!environment.IsDevelopment() && !EnvTypeHelper.IsTestEnv)
                    env = serviceOptions.Env?.ToLower() switch
                    {
                        "dev" => EnvType.DEV,
                        "qa" => EnvType.QA,
                        "uat" => EnvType.UAT,
                        "beta" => EnvType.BETA,
                        "prod" => EnvType.PROD,
                        _ => EnvType.LOCAL
                    };

                if (loggerOptions is not null)
                    LoggerHelperEx.Configure(config!.MicroServiceId, env, loggerOptions, loggerConfiguration, config!.Logger.CustomSetup);
            });

            var options = config = new OIUServicesConfiguration
            {
                CacheLock = new OIUServicesConfiguration.CacheLockSection(),
                Logger = new OIUServicesConfiguration.LoggerSection(),
                Configuration = new OIUServicesConfiguration.ConfigurationSection(),
                Database = new OIUServicesConfiguration.DatabaseSection(),
                CQRS = new OIUServicesConfiguration.CQRSSection(),
                EventBus = new OIUServicesConfiguration.EventSection(),
                Health = new OIUServicesConfiguration.HealthSection(),
                Security = new OIUServicesConfiguration.SecuritySection(),
                ApiServices = new OIUServicesConfiguration.ApiServicesSection(),
                Documentation = new OIUServicesConfiguration.DocumentationSection(),
                AspNet = new AspNetOptions(),
                OnInit = new OIUServicesConfiguration.InitSection()
            };
            setup(builder, config);
            //if (DocUtility.OneJNDoc(args, config.MicroServiceId, config.Documentation.Assemblies))
            //    return;

            builder.Services.AddOIUServices<TServiceOptions>(config);
            var app = builder.Build();

            // Update logger using the new one from serilog
            loggerFactory.Update(app.Services.GetRequiredService<Serilog.ILogger>());

            // app.UseSerilogRequestLogging();
            await app.OIUConfigureAsync<TServiceOptions>(compilation, config, exceptionHandler);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Host terminated unexpectedly.");
        }
        finally
        {
            loggerFactory.Dispose();
        }
    }

    #region Private Methods
    /// <summary>
    /// Crate console logger
    /// </summary>
    /// <returns></returns>
    private static Serilog.Core.Logger CreateConsoleLogger() => new LoggerConfiguration()
        .WriteTo.Console(outputTemplate: Constants.OutputTemplate, theme: AnsiConsoleTheme.Code)
        .CreateLogger();
    #endregion
}
