using Microsoft.Extensions.DependencyInjection;

namespace SoftUnlimit.MultiTenant.DependencyInjection
{
    /// <summary>
    /// Define a tenant configuration
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITenantConfigureServices<T> where T : Tenant
    {
        /// <summary>
        /// COnfigure service for some specific tenant.
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="services"></param>
        void ConfigureTenantServices(T tenant, IServiceCollection services);
    }
}
