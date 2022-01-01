using SoftUnlimit.MultiTenant.ResolutionStrategy;
using SoftUnlimit.MultiTenant.Store;
using System;

namespace SoftUnlimit.MultiTenant;

/// <summary>
/// Allow access to the current tenant.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITenantAccessService<T> where T : Tenant
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    T? GetTenant();
}
/// <summary>
/// Tenant access service
/// </summary>
/// <typeparam name="T"></typeparam>
public class TenantAccessService<T> : ITenantAccessService<T> where T : Tenant
{
    private readonly ITenantStore<T> _store;
    private readonly ITenantResolutionStrategy _resolutionStrategy;

    /// <summary>
    /// Build a resolution strategy
    /// </summary>
    /// <param name="resolutionStrategy"></param>
    /// <param name="store"></param>
    public TenantAccessService(ITenantResolutionStrategy resolutionStrategy, ITenantStore<T> store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _resolutionStrategy = resolutionStrategy ?? throw new ArgumentNullException(nameof(resolutionStrategy));
    }

    /// <summary>
    /// Get the current tenant
    /// </summary>
    /// <returns></returns>
    public T? GetTenant()
    {
        var tenantIdentifier = _resolutionStrategy.GetIdentifier();
        if (tenantIdentifier is null)
            return null;

        return _store.GetTenantAsync(tenantIdentifier).GetAwaiter().GetResult();
    }
}