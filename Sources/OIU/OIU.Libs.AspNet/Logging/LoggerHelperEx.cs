using Elastic.CommonSchema;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using OIU.Libs.AspNet.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Serilog.Sinks.SystemConsole.Themes;
using System.Data;
using UnlimitSoft;
using UnlimitSoft.Logger;
using UnlimitSoft.Text.Json.Logger;

namespace OIU.Libs.AspNet.Logging;


/// <summary>
/// 
/// </summary>
public static class LoggerHelperEx
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceId"></param>
    /// <param name="envType"></param>
    /// <param name="options"></param>
    /// <param name="loggerConfiguration"></param>
    /// <param name="setup"></param>
    public static void Configure(ushort serviceId, EnvType envType, LoggerOptions options, LoggerConfiguration loggerConfiguration, Action<LoggerConfiguration>? setup = null)
    {
        const string
            OIU = "OIU", System = "System", UnlimitSoft = "UnlimitSoft", UnlimitSoftSecurity = "UnlimitSoft.Web.AspNet.Security",
            Hangfire = "Hangfire", Microsoft = "Microsoft", ExceptionHandlerMiddleware = "Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware";

        var @override = options.LogLevel ??= [];

        #region Set defaults logging levels
        if (!@override.ContainsKey(System)) @override[System] = LogLevel.Warning;

        if (!@override.ContainsKey(OIU)) @override[OIU] = LogLevel.Information;

        if (!@override.ContainsKey(UnlimitSoft)) @override[UnlimitSoft] = LogLevel.Information;
        if (!@override.ContainsKey(UnlimitSoftSecurity)) @override[UnlimitSoftSecurity] = LogLevel.Warning;

        if (!@override.ContainsKey(Hangfire)) @override[Hangfire] = LogLevel.Warning;

        if (!@override.ContainsKey(Microsoft)) @override[Microsoft] = LogLevel.Warning;
        if (!@override.ContainsKey(ExceptionHandlerMiddleware)) @override[ExceptionHandlerMiddleware] = LogLevel.Critical;
        #endregion

        var config = new UnlimitSoft.Logger.Configuration.LoggerOption
        {
            Default = @override.Default,
            Override = @override.GetWithoutDefault()
        };
        LoggerHelper.Configure(config, loggerConfiguration, serviceId, EnvTypeHelper.Name);
        loggerConfiguration.Destructure.With(new JsonElementPolicy());

        setup?.Invoke(loggerConfiguration);

        if (options.Console is not null && options.Console.LogLevel.Default != LogLevel.None)
        {
            loggerConfiguration.WriteTo.Console(
                restrictedToMinimumLevel: (Serilog.Events.LogEventLevel)options.Console.LogLevel.Default,
                theme: AnsiConsoleTheme.Code,
                outputTemplate: Constants.OutputTemplate
            );
        }
        if (options.File is not null && options.File.LogLevel.Default != LogLevel.None)
        {
            loggerConfiguration.WriteTo.File(
                restrictedToMinimumLevel: (Serilog.Events.LogEventLevel)options.File.LogLevel.Default,
                path: $"logs/{EnvTypeHelper.Name}-{SysClock.GetNow():yyyyMMdd-HHmmss}.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 100 * 1024 * 1024,
                outputTemplate: Constants.OutputTemplate,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(2)
            );
        }
        if (options.MSSqlServer is not null && options.MSSqlServer.LogLevel.Default != LogLevel.None)
        {
            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = [
                    new SqlColumn
                    {
                        PropertyName = global::UnlimitSoft.Constants.LogContextCorrelationId,
                        ColumnName = global::UnlimitSoft.Constants.LogContextCorrelationId,
                        DataType = SqlDbType.NVarChar,
                        DataLength = 40,
                        AllowNull = true,
                        NonClusteredIndex = true,
                    },
                    new SqlColumn
                    {
                        PropertyName = Constants.LogContextIdentity,
                        ColumnName = Constants.LogContextIdentity,
                        DataType = SqlDbType.UniqueIdentifier,
                        AllowNull = true,
                        NonClusteredIndex = true,
                    }
                ]
            };

            columnOptions.Level.NonClusteredIndex = true;
            columnOptions.Level.DataType = SqlDbType.TinyInt;

            columnOptions.TimeStamp.NonClusteredIndex = true;

            // Create SQL Connection
            loggerConfiguration.WriteTo.MSSqlServer(options.MSSqlServer.ConnectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    SchemaName = "logger",
                    TableName = options.MSSqlServer.TableName,
                    AutoCreateSqlTable = true,
                },
                restrictedToMinimumLevel: (Serilog.Events.LogEventLevel)options.MSSqlServer.LogLevel.Default,
                columnOptions: columnOptions
            );
        }
        if (options.ApplicationInsight is not null && options.ApplicationInsight.LogLevel.Default != LogLevel.None)
        {
            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.ConnectionString = options.ApplicationInsight.ConnectionString;

            loggerConfiguration.WriteTo.ApplicationInsights(
                new TelemetryClient(telemetryConfig),
                TelemetryConverter.Traces,
                restrictedToMinimumLevel: (Serilog.Events.LogEventLevel)options.ApplicationInsight.LogLevel.Default
            );
        }
        if (envType != EnvType.LOCAL && envType != EnvType.TEST && options.ElasticSearch is not null && options.ElasticSearch.LogLevel.Default != LogLevel.None)
        {
            loggerConfiguration.Enrich.WithProperty("Env", (int)envType);
            /// TODO: update to use the new ElasticSearch sink
            //loggerConfiguration.WriteTo.Elasticsearch(
            //    [new Uri(options.ElasticSearch.Url)],
            //    opts =>
            //    {
            //        opts.DataStream = new DataStreamName("onejn", "logs", envType.ToString().ToLower());

            //        opts.BootstrapMethod = BootstrapMethod.Silent;
            //        opts.MinimumLevel = (Serilog.Events.LogEventLevel)options.ElasticSearch.LogLevel.Default;
            //        opts.ConfigureChannel = channelOpts => channelOpts.BufferOptions = new Elastic.Channels.BufferOptions { ExportMaxConcurrency = 10 };

            //        opts.TextFormatting.MapCustom = MapCustom;
            //    },
            //    t =>
            //    {
            //        t.Authentication(new ApiKey(options.ElasticSearch.ApiKey));
            //        t.ServerCertificateValidationCallback((sender, cert, chain, error) => true);
            //    }
            //);
        }
    }

    #region Private Methods
    private static EcsDocument MapCustom(EcsDocument document, Serilog.Events.LogEvent @event)
    {
        if (document.Labels is null || document.Metadata is null)
            return document;

        foreach (var metadata in document.Metadata)
        {
            if (metadata.Value is Enum @enum)
            {
                document.Metadata.Remove(metadata.Key);
                document.Labels[metadata.Key] = @enum.ToString();
                continue;
            }
            if (document.Metadata[metadata.Key] is Dictionary<string, object> dict && dict.TryGetValue(string.Empty, out var dictValue))
            {
                dict["_"] = dictValue;
                dict.Remove(string.Empty);
                continue;
            }
        }
        return document;
    }
    #endregion
}