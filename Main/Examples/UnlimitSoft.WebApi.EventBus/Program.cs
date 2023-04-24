using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.EventBus.Azure;
using UnlimitSoft.EventBus.DotNetMQ;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.Configuration;
using UnlimitSoft.Logger.DependencyInjection;
using UnlimitSoft.Text.Json;
using UnlimitSoft.WebApi.EventBus;
using UnlimitSoft.WebApi.EventBus.EventBus;
using UnlimitSoft.WebApi.EventBus.Processors;

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

var app = ConfigureServices(builder.Services, EventBusType.Azure);
await Configure(app);


WebApplication ConfigureServices(IServiceCollection services, EventBusType busType)
{
    #region Config
    var section = busType switch
    {
        EventBusType.Azure => "AzureEventBus",
        EventBusType.DotNetMQ => "DotNetMQEventBus",
        _ => throw new NotSupportedException()
    };
    services.Configure<EventBusOptions>(builder.Configuration.GetSection(section));
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

    services.AddSingleton(JsonUtil.Default);

    #region Event Bus
    services.AddUnlimitSoftEventNameResolver(new [] { typeof(Program).Assembly });
    switch (busType)
    {
        case EventBusType.Azure:
            AddAzureEventBus(services);
            break;
        case EventBusType.DotNetMQ:
            AddDotNetMQEventBus(services);
            break;
        default:
            throw new NotSupportedException("Data is not supported");
    }
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

IServiceCollection AddAzureEventBus(IServiceCollection services)
{
    services.AddSingleton<IEventBus>(provider =>
    {
        var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;
        var logger = provider.GetRequiredService<ILogger<AzureEventBus<QueueIdentifier>>>();
        var eventNameResolver = provider.GetRequiredService<IEventNameResolver>();
        var serialize = provider.GetRequiredService<IJsonSerializer>();

        return new AzureEventBus<QueueIdentifier>(
           options.Endpoint,
           options.PublishQueues,
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
        var serialize = provider.GetRequiredService<IJsonSerializer>();
        var resolver = provider.GetRequiredService<IEventNameResolver>();
        var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;
        var logger = provider.GetRequiredService<ILogger<AzureEventListener<QueueIdentifier>>>();

        return new AzureEventListener<QueueIdentifier>(
            options.Endpoint,
            options.ListenQueues,
            (arg, ct) => DefaultProcessor.ProcessAsync(arg, logger, ct),
            serialize,
            1,
            logger
        );
    });
    return services;
}
IServiceCollection AddDotNetMQEventBus(IServiceCollection services)
{
    services.AddSingleton<IEventBus>(provider =>
    {
        var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;
        var logger = provider.GetRequiredService<ILogger<MemoryEventBus<QueueIdentifier>>>();
        var eventNameResolver = provider.GetRequiredService<IEventNameResolver>();
        var serialize = provider.GetRequiredService<IJsonSerializer>();

        return new MemoryEventBus<QueueIdentifier>(
           "Sender",
           options.PublishQueues,
           eventNameResolver,
           serialize,
           null,
           null,
           setup: (graph, message) =>
           {
               //message.ApplicationProperties["IdentityId"] = "Me";       // Set identity as custom property in the event
           },
           logger
        );
    });
    services.AddSingleton<IEventListener>(provider =>
    {
        var serialize = provider.GetRequiredService<IJsonSerializer>();
        var resolver = provider.GetRequiredService<IEventNameResolver>();
        var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;
        var logger = provider.GetRequiredService<ILogger<MemoryEventListener<QueueIdentifier>>>();

        return new MemoryEventListener<QueueIdentifier>(
            options.ListenQueues!,
            (arg, ct) => DefaultProcessor.ProcessAsync(arg, logger, ct),
            serialize,
            logger
        );
    });
    return services;
}