using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SoftUnlimit.Logger.Configuration;
using SoftUnlimit.Logger.DependencyInjection;
using SoftUnlimit.Logger.Web;

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
Configure(app);


WebApplication ConfigureServices(IServiceCollection services)
{
    // Add services to the container.
    #region Logger
    services.AddUnlimitSofLogger(
        new LoggerOption {
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


    app.UseMiddleware<LoggerMiddleware>();

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