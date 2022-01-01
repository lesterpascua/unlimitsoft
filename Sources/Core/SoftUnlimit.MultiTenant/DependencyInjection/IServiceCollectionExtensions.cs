using Microsoft.Extensions.DependencyInjection;

namespace SoftUnlimit.MultiTenant.DependencyInjection;

/// <summary>
/// Nice method to create the tenant builder
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Add the services (application specific tenant class)
    /// </summary>
    /// <typeparam name="T">Tenant used in the process.</typeparam>
    /// <typeparam name="TService">The type of the implementation to use for <see cref="ITenantConfigureServices{T}"/></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder<T> AddMultiTenancy<T, TService>(this IServiceCollection services) where T : Tenant where TService : class, ITenantConfigureServices<T>
    {
        services.AddSingleton<ITenantConfigureServices<T>, TService>();
        return new(services);
    }
    /// <summary>
    /// Add the services (default tenant class)
    /// </summary>
    /// <typeparam name="TService">The type of the implementation to use.</typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder<Tenant> AddMultiTenancy<TService>(this IServiceCollection services) where TService : class, ITenantConfigureServices<Tenant>
    {
        services.AddSingleton<ITenantConfigureServices<Tenant>, TService>();
        return new(services);
    }
}
