using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoftUnlimit.MultiTenant.DependencyInjection;

namespace SoftUnlimit.MultiTenant.AspNet
{
    /// <summary>
    /// Utility to register tenant provider.
    /// </summary>
    public static class IHostBuilderExtensions
    {
        /// <summary>
        /// Overrride the provide to use tenant provider.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="validateOnBuild"></param>
        /// <param name="validateScopes"></param>
        /// <returns></returns>
        public static IHostBuilder UseTenantServiceProviderFactory(this IHostBuilder builder, bool validateOnBuild = true, bool validateScopes = true)
        {
            return builder.UseServiceProviderFactory(context =>
                new TenantServiceProviderFactory(new ServiceProviderOptions { ValidateOnBuild = validateOnBuild, ValidateScopes = validateScopes })
            );
        }
    }
}