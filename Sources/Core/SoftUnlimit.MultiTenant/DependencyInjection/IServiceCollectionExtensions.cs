using Microsoft.Extensions.DependencyInjection;

namespace SoftUnlimit.MultiTenant.DependencyInjection;

/// <summary>
/// Nice method to create the tenant builder
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Add the services (default tenant class)
    /// </summary>
    /// <typeparam name="TService">The type of the implementation to use.</typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder AddMultiTenancy<TService>(this IServiceCollection services) where TService : class, ITenantConfigureServices<Tenant>
    {
        services.AddSingleton<ITenantConfigureServices<Tenant>, TService>();
        return new(services);
    }
}
