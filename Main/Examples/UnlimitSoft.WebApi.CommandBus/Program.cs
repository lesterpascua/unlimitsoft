using Serilog;
using Serilog.Context;
using Serilog.Sinks.SystemConsole.Themes;
using UnlimitSoft;
using UnlimitSoft.Bus.Hangfire;
using UnlimitSoft.Bus.Hangfire.DependencyInjection;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.Configuration;
using UnlimitSoft.Logger.DependencyInjection;
using UnlimitSoft.Text.Json;
using UnlimitSoft.WebApi.CommandBus.Commands;

// ================================================================================================================================
// In this example we add a event bus and comunicate events between sender and listener
// Steps
//  - Inject AddUnlimitSoftEventNameResolver 
//  - IEventBus
//  - IEventListener
//      
// ================================================================================================================================

// Entry Point
JsonUtil.Default = new DefaultJsonSerializer();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var app = ConfigureServices(builder.Services);
Configure(app);


WebApplication ConfigureServices(IServiceCollection services)
{
    #region Config
    var connString = builder.Configuration.GetConnectionString("Local")!;
    #endregion

    // Add services to the container.
    #region Logger
    var loggerConfiguration = new LoggerConfiguration();
    loggerConfiguration.WriteTo.Console(
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{IdentityId} {CorrelationId}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"
    );
    UnlimitSoft.Logger.LoggerHelper.Configure(
        new LoggerOption
        {
            Override = new Dictionary<string, LogLevel> { { "Microsoft", LogLevel.Warning } }
        },
        loggerConfiguration,
        1,
        builder.Environment.EnvironmentName
    );
    #endregion

    services.AddSingleton(JsonUtil.Default);

    #region CQRS
    services.AddUnlimitSoftCQRS(
        new CQRSSettings
        {
            Assemblies = [typeof(Program).Assembly],
            ICommandHandler = typeof(ICommandHandler<,>),
        }
    );
    #endregion
    #region Command Bus
    var hangfireOptions = new HangfireOptions
    {
        ConnectionString = connString,
        Logger = Hangfire.Logging.LogLevel.Info,
        SchedulePollingInterval = TimeSpan.FromSeconds(15),
        Scheme = "hf",
        WorkerCount = 1
    };
    services.AddHangfireCommandBus<SchedulerCommandProps>(
        hangfireOptions,
        middleware: async (provider, command, context, next, ct) =>
        {
            var meta = context.BackgroundJob;
            var correlationId = Guid.NewGuid().ToString();

            using var _1 = LogContext.PushProperty(Constants.LogContextCorrelationId, correlationId);
            return await next(command, ct);
        }
    );
    #endregion

    services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    return builder.Build();
}
void Configure(WebApplication app)
{
#if DEBUG
    const string compilation = "DEBUG";
#else
    const string compilation = "RELEASE";
#endif

    app.Services
        .GetRequiredService<ILogger<Program>>()
        .LogInformation("Starting, ENV: {Environment}, COMPILER: {Compilation} ...", app.Environment.EnvironmentName, compilation);


    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}