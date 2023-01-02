using UnlimitSoft.MultiTenant;
using UnlimitSoft.MultiTenant.DependencyInjection;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenants;


public sealed class MyTenantConfigure : ITenantConfigure
{
    private readonly IServiceScopeFactory _factory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MyTenantConfigure> _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="environment"></param>
    /// <param name="logger"></param>
    public MyTenantConfigure(IServiceScopeFactory factory, IWebHostEnvironment environment, ILogger<MyTenantConfigure> logger)
    {
        _factory = factory;
        _environment = environment;
        _logger = logger;
    }

    public void Configure(Tenant tenant)
    {
#if DEBUG
        const string compilation = "DEBUG";
#else
    const string compilation = "RELEASE";
#endif

        _logger.LogInformation("Starting, ENV: {Environment}, COMPILER: {Compilation}, TENANT: {Tenant} ...", _environment.EnvironmentName, compilation, tenant.Id);

        // Startup tenant operation here
    }
}
