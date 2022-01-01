namespace SoftUnlimit.MultiTenant.ResolutionStrategy;

/// <summary>
/// Define a resolution strategy to get the current tenant identifier.
/// </summary>
public interface ITenantResolutionStrategy
{
    /// <summary>
    /// Get the identifier of the tenant.
    /// </summary>
    /// <returns></returns>
    string? GetIdentifier();
}