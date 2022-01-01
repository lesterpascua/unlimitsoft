using Microsoft.AspNetCore.Builder;

namespace SoftUnlimit.MultiTenant.AspNet;

/// <summary>
/// Nice method to register our middleware
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Use the Teanant Middleware to process the request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenancy<T>(this IApplicationBuilder builder) where T : Tenant => builder.UseMiddleware<MultiTenantMiddleware<T>>();
    /// <summary>
    /// Use the Teanant Middleware to process the request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder) => builder.UseMiddleware<MultiTenantMiddleware<Tenant>>();
}
