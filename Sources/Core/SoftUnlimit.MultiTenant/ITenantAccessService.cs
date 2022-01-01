using SoftUnlimit.MultiTenant.ResolutionStrategy;
using SoftUnlimit.MultiTenant.Store;
using System;

namespace SoftUnlimit.MultiTenant;

/// <summary>
/// Allow access to the current tenant.
/// </summary>
public interface ITenantAccessService
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Tenant? GetTenant();
}
/// <summary>
/// Tenant access service
/// </summary>
public class TenantAccessService : ITenantAccessService
{
    private readonly ITenantStore<Tenant> _store;
    private readonly ITenantResolutionStrategy _resolutionStrategy;

    /// <summary>
    /// Build a resolution strategy
    /// </summary>
    /// <param name="resolutionStrategy"></param>
    /// <param name="store"></param>
    public TenantAccessService(ITenantResolutionStrategy resolutionStrategy, ITenantStore<Tenant> store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _resolutionStrategy = resolutionStrategy ?? throw new ArgumentNullException(nameof(resolutionStrategy));
    }

    /// <summary>
    /// Get the current tenant
    /// </summary>
    /// <returns></returns>
    public Tenant? GetTenant()
    {
        var key = _resolutionStrategy.GetKey(out var tenant);

        if (tenant is not null)
            return tenant;
        if (key is null)
            return null;

        return _store.GetTenantAsync(key).GetAwaiter().GetResult();
    }
}