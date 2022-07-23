using UnlimitSoft.MultiTenant.AspNet;
using System;

namespace UnlimitSoft.MultiTenant.ResolutionStrategy;

/// <summary>
/// Resolve the host to a tenant identifier
/// </summary>
public class HostResolutionStrategy : ITenantResolutionStrategy
{
    private readonly ITenantContextAccessor _accessor;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accessor"></param>
    public HostResolutionStrategy(ITenantContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// Get the tenant identifier
    /// </summary>
    /// <param name="tenant"></param>
    /// <returns></returns>
    public string? GetKey(out Tenant? tenant)
    {
        tenant = null;
        var context = _accessor.GetContext() as TenantHttpContext;
        return context?.Context?.Request.Host.Host;
    }
}