using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.DependencyInjection;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.Event.Json;
using UnlimitSoft.CQRS.Memento;
using UnlimitSoft.CQRS.Memento.Json;
using UnlimitSoft.Data.EntityFramework.DependencyInjection;
using UnlimitSoft.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.Configuration;
using UnlimitSoft.Logger.DependencyInjection;
using UnlimitSoft.Security;
using UnlimitSoft.Text.Json;
using UnlimitSoft.WebApi.EventSourced.Client;
using UnlimitSoft.WebApi.EventSourced.CQRS;
using UnlimitSoft.WebApi.EventSourced.CQRS.BPL;
using UnlimitSoft.WebApi.EventSourced.CQRS.Configuration;
using UnlimitSoft.WebApi.EventSourced.CQRS.Data;
using UnlimitSoft.WebApi.EventSourced.CQRS.Event;
using UnlimitSoft.WebApi.EventSourced.CQRS.Model;
using UnlimitSoft.WebApi.EventSourced.CQRS.Repository;

// ================================================================================================================================
// In this example we use event sourced to store order entity
// Steps
//  - Inject IMyIdGenerator to generate unique key for the services
//  - Inject AddUnlimitSoftDefaultFrameworkUnitOfWork this will register entities event source repository and all entity framework dependency
//  - Inject IMemento<IOrder> to access order entity
//  - Inject AddUnlimitSoftEventNameResolver to resolve event type from event name
// ================================================================================================================================

// Entry Point
JsonUtil.Default = new DefaultJsonSerializer();
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

var app = ConfigureServices(builder.Services);
Configure(app);


WebApplication ConfigureServices(IServiceCollection services)
{
    string connString = "MemoryDb";

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

    #region Config
    services.AddSingleton(JsonUtil.Default);
    #endregion

    #region CQRS
    var gen = new MyIdGenerator(1);

    services.AddSingleton<IMyIdGenerator>(gen);
    services.AddSingleton<IServiceMetadata>(gen);

    services.AddUnlimitSoftDefaultFrameworkUnitOfWork(
        new UnitOfWorkOptions
        {
            EntityTypeBuilder = typeof(_EntityTypeBuilder<>),
            IUnitOfWork = typeof(IMyUnitOfWork),
            UnitOfWork = typeof(MyUnitOfWork),
            DbContextWrite = typeof(DbContextWrite),
            PoolSizeForWrite = 128,
            WriteConnString = connString,
            WriteBuilder = (options, builder, connString) => InitHelper.SQLWriteBuilder<DbContextWrite>(connString, builder),

            EventSourcedRepository = typeof(MyEventRepository),
            IEventSourcedRepository = typeof(IMyEventRepository),
        }
    );
    services.AddScoped<IMemento<IOrder>>(provider =>
    {
        var serializer = provider.GetRequiredService<IJsonSerializer>();
        var nameResolver = provider.GetRequiredService<IEventNameResolver>();
        var eventSourcedRepository = provider.GetRequiredService<IMyEventRepository>();

        return new MyMemento<IOrder, Order>(
            serializer,
            nameResolver,
            eventSourcedRepository,
            historicalEvents => new Order(historicalEvents)
        );
    });
    #endregion

    #region EventBus
    services.AddUnlimitSoftEventNameResolver(new Assembly[] {
        typeof(MyEvent<>).Assembly
    });
    #endregion

    services.AddScoped<OrderService>();

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
