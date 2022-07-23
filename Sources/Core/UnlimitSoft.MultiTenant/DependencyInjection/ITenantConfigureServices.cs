using Microsoft.Extensions.DependencyInjection;

namespace UnlimitSoft.MultiTenant.DependencyInjection
{
    /// <summary>
    /// Define a tenant configuration
    /// </summary>
    public interface ITenantConfigureServices
    {
        /// <summary>
        /// COnfigure service for some specific tenant.
        /// </summary>
        /// <param name="tenant"></param>
        /// <param name="services"></param>
        void ConfigureTenantServices(Tenant tenant, IServiceCollection services);
    }
}
