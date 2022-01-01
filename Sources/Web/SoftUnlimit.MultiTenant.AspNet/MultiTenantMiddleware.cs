using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.MultiTenant.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SoftUnlimit.MultiTenant.AspNet;

/// <summary>
/// <list>
///     <item>Populate the <see cref="Constants.HttpContextTenantKey"/> with the current tenant.</item>
///     <item>Replace <see cref="HttpContext.RequestServices"/> with the tenant service provider.</item>
/// </list>
/// </summary>
/// <typeparam name="T"></typeparam>
public class MultiTenantMiddleware<T> where T : Tenant
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    public MultiTenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Step in the middleware.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="multiTenantContainerAccessor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task Invoke(HttpContext context, Func<TenantServiceProvider> multiTenantContainerAccessor)
    {
        var tenant = PopulateAccessor(context);
        await NextUsingTenantScope(context, multiTenantContainerAccessor, tenant);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual TenantContext BuildTenantMetadata(HttpContext context) => new TenantHttpContext(context);

    #region Private Methods
    private Tenant? PopulateAccessor(HttpContext context)
    {
        if (context.Items.TryGetValue(Constants.HttpContextTenantKey, out var value))
            return (Tenant)value;

        var tenantAccessor = (TenantContextAccessor)context.RequestServices.GetRequiredService<ITenantContextAccessor>();
        var metadata = BuildTenantMetadata(context);
        tenantAccessor.SetContext(metadata);

        var tenantService = context.RequestServices.GetRequiredService<ITenantAccessService>();
        var tenant = tenantService.GetTenant();

        if (tenant is not null)
            tenantAccessor.SetContext(new TenantCloneContext(tenant));

        context.Items.Add(Constants.HttpContextTenantKey, tenant);
        return tenant;
    }
    private async Task NextUsingTenantScope(HttpContext context, Func<TenantServiceProvider> multiTenantContainerAccessor, Tenant? tenant)
    {
        if (tenant is null)
        {
            // Continue processing
            if (_next is not null)
                await _next(context);

            return;
        }

        // Set to current tenant container.
        // Begin new scope for request as ASP.NET Core standard scope is per-request
        var root = multiTenantContainerAccessor();
        if (root is null)
            throw new InvalidOperationException("Can't resolve root tenant");

        var prevProvider = context.RequestServices;
        using var scope = root.GetRequiredService<IServiceProvider>().CreateScope();

        context.RequestServices = scope.ServiceProvider;

        // Continue processing
        if (_next is not null)
            await _next(context);
        context.RequestServices = prevProvider;
    }
    #endregion
}