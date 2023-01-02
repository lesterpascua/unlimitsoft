using Microsoft.AspNetCore.Builder;

namespace UnlimitSoft.MultiTenant.AspNet;

/// <summary>
/// Nice method to register our middleware
/// </summary>
public static class IApplicationBuilderExtensions
{
    /// <summary>
    /// Use the Teanant Middleware to process the request. Define this at top of the middleware to waranty all the pipeline run asociate to the correct tenant.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder builder) => builder.UseMiddleware<MultiTenantMiddleware<Tenant>>();
    /// <summary>
    /// Use the Teanant Middleware to process the request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMultiTenancy<T>(this IApplicationBuilder builder) where T : Tenant => builder.UseMiddleware<MultiTenantMiddleware<T>>();
}
