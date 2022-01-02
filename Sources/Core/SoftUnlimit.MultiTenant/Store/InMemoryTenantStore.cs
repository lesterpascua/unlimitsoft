using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.MultiTenant.Store;

/// <summary>
/// In memory store. If the tenant not found use the <see cref="String.Empty"/> value
/// </summary>
public class InMemoryTenantStore : ITenantStore
{
    private readonly IDictionary<string, Tenant> _tenants;

    /// <summary>
    /// Initialize new instance.
    /// </summary>
    /// <param name="tenants"></param>
    public InMemoryTenantStore(IEnumerable<Tenant> tenants)
    {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        _tenants = tenants
            .Where(p => p.Key is not null)
            .ToDictionary(k => k.Key);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }

    /// <summary>
    /// Get a tenant for a given identifier
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public Tenant GetTenant(string identifier)
    {
        if (!_tenants.TryGetValue(identifier, out var tenant) && !_tenants.TryGetValue(string.Empty, out tenant))
            throw new KeyNotFoundException($"Key {identifier} not found");

        return tenant;
    }
}