using Microsoft.Extensions.DependencyInjection;

namespace UnlimitSoft.MultiTenant.DependencyInjection;


/// <summary>
/// Nice method to create the tenant builder
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Register <see cref="ITenantConfigureServices"/> with the service in the generic parameter. This is register as singlenton.
    /// </summary>
    /// <remarks>
    /// Register this at the end of all other registration this tenant will clone the common services register to adapt inside of the 
    /// tenant. Tenant will keep reference of all top level register service to later user as tenant level. If you register as top level a singlenton
    /// object this will be visible inside of the tenant as singlenton but instance will be shared across all other tenant. Take this into account because you
    /// mign be shared data between tenant you don't want to.
    /// </remarks>
    /// <typeparam name="TService">Concrete type implement <see cref="ITenantConfigureServices"/></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static TenantBuilder AddMultiTenancy<TService>(this IServiceCollection services) where TService : class, ITenantConfigureServices
    {
        services.AddSingleton<ITenantConfigureServices, TService>();
        return new(services);
    }
}
