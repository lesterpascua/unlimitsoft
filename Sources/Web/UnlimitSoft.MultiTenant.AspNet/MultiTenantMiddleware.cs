using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace UnlimitSoft.MultiTenant.AspNet;


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
    private readonly IRootTenantServiceProvider _multiTenantRootServiceProvider;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="next"></param>
    /// <param name="multiTenantRootServiceProvider"></param>
    public MultiTenantMiddleware(RequestDelegate next, IRootTenantServiceProvider multiTenantRootServiceProvider)
    {
        _next = next;
        _multiTenantRootServiceProvider = multiTenantRootServiceProvider;
    }

    /// <summary>
    /// Step in the middleware.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task Invoke(HttpContext context)
    {
        var tenant = PopulateAccessor(context);
        await NextUsingTenantScope(context, tenant);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected virtual TenantContext CreateTenantContext(HttpContext context) => new TenantHttpContext(context);

    #region Private Methods
    /// <summary>
    /// In this method we will create the <see cref="TenantContext"/> with the necessary information to build the current tenant.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private Tenant? PopulateAccessor(HttpContext context)
    {
        if (context.Items.TryGetValue(Constants.HttpContextTenantKey, out var value))
            return (Tenant)value;

        var tenantAccessor = (TenantContextAccessor)context.RequestServices.GetRequiredService<ITenantContextAccessor>();
        var tenantContext = CreateTenantContext(context);
        tenantAccessor.SetContext(tenantContext);

        var tenantService = context.RequestServices.GetRequiredService<ITenantAccessService>();
        var tenant = tenantService.GetTenant();

        if (tenant is not null)
            tenantContext.Tenant = tenant;

        context.Items.Add(Constants.HttpContextTenantKey, tenant);
        return tenant;
    }
    private async Task NextUsingTenantScope(HttpContext context, Tenant? tenant)
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
        var root = _multiTenantRootServiceProvider.GetProvider();
        if (root is null)
            throw new InvalidOperationException("Can't resolve root tenant");

        //
        // Create tenant scope to execute asp.net request inside of the current
        // scope. All instance create until this moment will be injerit by the current scope
        using var scope = root.GetRequiredService<IServiceProvider>().CreateScope();

        var prevProvider = context.RequestServices;
        context.RequestServices = scope.ServiceProvider;
        try
        {
            // Continue processing
            if (_next is not null)
                await _next(context);
        }
        finally
        {
            context.RequestServices = prevProvider;
        }
    }
    #endregion
}