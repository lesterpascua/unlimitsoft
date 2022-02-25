using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SoftUnlimit.Logger.Configuration;
using SoftUnlimit.Logger.DependencyInjection;
using SoftUnlimit.Logger.Web;
using UnlimitSoft.WebApi.Logger.Logger;

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
#if DEBUG
    string compilation = "DEBUG";
#else
    string compilation = "RELEASE";
#endif

    // Add services to the container.
    #region Logger
    builder.Services.AddUnlimitSofLogger(
        new LoggerOption { },
        builder.Environment.EnvironmentName,
        compilation,
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


    builder.Services.AddControllers(c =>
    {
        c.Filters.Add(typeof(CustomIdentityAttribute));
    });
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    return builder.Build();
}
void Configure(WebApplication app)
{
    app.UseMiddleware<LoggerMiddleware<MyLoggerContext>>();

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