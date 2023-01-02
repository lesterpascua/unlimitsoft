using Microsoft.Extensions.Options;
using UnlimitSoft.MultiTenant;
using UnlimitSoft.MultiTenant.DependencyInjection;
using UnlimitSoft.WebApi.MultiTenant.Sources.Configuration;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.MultiTenants;


public sealed class MyTenantConfigureServices : ITenantConfigureServices
{
    private readonly IOptions<ServiceOptions> _serviceOptions;

    public MyTenantConfigureServices(IOptions<ServiceOptions> serviceOptions)
    {
        _serviceOptions = serviceOptions;
    }


    public void ConfigureTenantServices(Tenant tenant, IServiceCollection services)
    {
        services.AddSingleton<ITenantConfigure, MyTenantConfigure>();
    }
}
