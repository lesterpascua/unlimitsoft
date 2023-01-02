namespace UnlimitSoft.MultiTenant.ResolutionStrategy;


/// <summary>
/// Define a resolution strategy to get the current tenant identifier.
/// </summary>
public interface ITenantResolutionStrategy
{
    /// <summary>
    /// Get the identifier of the tenant.
    /// </summary>
    /// <param name="tenant">For optimization the identifier can return the tenantId to avoid search in the storage.</param>
    /// <returns></returns>
    string? GetKey(out Tenant? tenant);
}