using Microsoft.Extensions.DependencyInjection;

namespace UnlimitSoft.MultiTenant.DependencyInjection;


/// <summary>
/// Define a tenant configuration.
/// </summary>
/// <remarks>
/// Tenant configuration is similar to ConfigureServices(IServiceCollection services) in standard net core application. Allow register at tenant level. Service could be singlenton, scoped or trasient but this will be per tenent.
/// Supose you have 2 tenant and you register a singlenton service this mean the service will have an unique instance per tenent but tenant 1 can't access tenant 2 instance.
/// </remarks>
public interface ITenantConfigureServices
{
    /// <summary>
    /// Configure service for some specific tenant.
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="services"></param>
    void ConfigureTenantServices(Tenant tenant, IServiceCollection services);
}
