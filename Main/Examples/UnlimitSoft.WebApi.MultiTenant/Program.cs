using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using UnlimitSoft.Json;
using UnlimitSoft.Logger.Configuration;
using UnlimitSoft.Logger.DependencyInjection;
using UnlimitSoft.MultiTenant;
using UnlimitSoft.MultiTenant.AspNet;
using UnlimitSoft.MultiTenant.DependencyInjection;
using UnlimitSoft.MultiTenant.ResolutionStrategy;
using UnlimitSoft.Text.Json;
using UnlimitSoft.WebApi.MultiTenant.Sources.Configuration;
using UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenant;
using UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenants;

// ================================================================================================================================
// In this example we add a multi tenant support
// Steps
//  - Inject AddUnlimitSoftEventNameResolver 
//  - IEventBus
//  - IEventListener
//      
// ================================================================================================================================

// Entry Point
JsonUtil.Default = new DefaultJsonSerializer();
var builder = WebApplication.CreateBuilder(args);
builder.Host
    .UseTenantServiceProviderFactory()
    .UseSerilog();

var app = ConfigureServices(builder.Services);
Configure(app);


WebApplication ConfigureServices(IServiceCollection services)
{
    #region Config
    //
    // Load default configuration in case of tenant not set any seettings
    services.Configure<ServiceOptions>(builder.Configuration.GetSection("Service"));
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

    #region ASP.NET
    services.AddControllers();
    #endregion

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    #region Multi Tenant
    //
    // Register this at the end of all other registration this tenant will clone the common services register to adapt inside of the tenant
    services
        .AddMultiTenancy<MyTenantConfigureServices>()
        .WithResolutionStrategy(p => new QSResolutionStrategy(p.GetRequiredService<ITenantContextAccessor>()))
        .WithStore<MyTenantStorage>()
        .WithTenantConfigure<ServiceOptions>(TimeSpan.MaxValue, (provider, options, tenant) =>
        {
            options.Client = tenant.Key;
        });
    #endregion

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
    
    app.UseMultiTenancy();

    app.MapControllers();

    app.Run();
}