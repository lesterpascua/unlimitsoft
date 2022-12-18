using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.EventBus.Azure;
using UnlimitSoft.EventBus.Azure.Configuration;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.Configuration;
using UnlimitSoft.Logger.DependencyInjection;
using UnlimitSoft.WebApi.EventBus.EventBus;

// ================================================================================================================================
// In this example we add a logger and include a custom properties in every logger
// Steps
//  - Inject AddUnlimitSofLogger this will inject a couple of serilog features
//      * LoggerContextEnricher: to add the custom properties
//      * ILoggerContextAccessor:  
//      * LoggerMiddleware: to set the trace and correlation properties
//      * Add custom attribute to register the IdentityId
// ================================================================================================================================

// Entry Point
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var app = ConfigureServices(builder.Services);
await Configure(app);


WebApplication ConfigureServices(IServiceCollection services)
{
    #region Config
    services.Configure<EventBusOptions>(builder.Configuration.GetSection("EventBus"));
    #endregion

    // Add services to the container.
    #region Logger
    services.AddUnlimitSofLogger(
        new LoggerOption
        {
            Override = new Dictionary<string, LogLevel> {
                { "Microsoft", LogLevel.Warning }
            }
        },
        builder.Environment.EnvironmentName,
        setup =>
        {
            setup.WriteTo.Console(
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                theme: AnsiConsoleTheme.Code,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{IdentityId} {CorrelationId}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}"
            );
        }
    );
    #endregion


    #region Event Bus
    services.AddUnlimitSoftEventNameResolver(new [] { typeof(Program).Assembly });
    services.AddSingleton<IEventBus>(provider =>
    {
        var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;
        var queues = new[] { 
            new QueueAlias<QueueIdentifier> { Active = true, Alias = QueueIdentifier.Test, Queue = options.QueueOrTopic }
        };

        var logger = provider.GetRequiredService<ILogger<Program>>();
        var eventNameResolver = provider.GetRequiredService<IEventNameResolver>();
        var serialize = provider.GetRequiredService<IJsonSerializer>();

        return new AzureEventBus<QueueIdentifier>(
           options.Endpoint,
           queues,
           eventNameResolver,
           serialize,
           null,
           null,
           setup: (graph, message) =>
           {
               message.ApplicationProperties["IdentityId"] = "Me";       // Set identity as custom property in the event
           },
           logger
        );
    });
    services.AddSingleton<IEventListener>(provider =>
    {
        var resolver = provider.GetRequiredService<IEventNameResolver>();
        var logger = provider.GetRequiredService<ILogger<AzureEventListener<QueueIdentifier>>>();
        var serialize = provider.GetRequiredService<IJsonSerializer>();
        var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;

        return new AzureEventListener<QueueIdentifier>(
            options.Endpoint,
            new[] { 
                new QueueAlias<QueueIdentifier> { Active = true, Alias = QueueIdentifier.TestService1, Queue = options.Queue1 },
                new QueueAlias<QueueIdentifier> { Active = true, Alias = QueueIdentifier.TestService2, Queue = options.Queue2 }
            },
            async (args, ct) =>
            {
                if (args.Envelop.Type != MessageType.Json)
                    args.Envelop.Type = MessageType.Json;

                var message = args.Azure.Message;
                if (!message.ApplicationProperties.TryGetValue("IdentityId", out var identityId))
                    identityId = null;

                logger.LogDebug("Receive from {Queue}, event: {@Event}", args.Queue, args.Envelop);
                try
                {
                    logger.LogInformation("Event: {@Args}", args);
                    await args.Azure.CompleteMessageAsync(message, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Event: {@Args}", args);
                    await args.Azure.CompleteMessageAsync(message, ct);
                }
            },
            serialize,
            1,
            logger
        );
    });
    #endregion

    services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    return builder.Build();
}
async Task Configure(WebApplication app)
{
#if DEBUG
    const string compilation = "DEBUG";
#else
    const string compilation = "RELEASE";
#endif

    app.Services
        .GetRequiredService<ILogger<Program>>()
        .LogInformation("Starting, ENV: {Environment}, COMPILER: {Compilation} ...", app.Environment.EnvironmentName, compilation);
    await app.Services.GetRequiredService<IEventBus>().StartAsync(TimeSpan.FromSeconds(5));
    await app.Services.GetRequiredService<IEventListener>().ListenAsync(TimeSpan.FromSeconds(5));


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