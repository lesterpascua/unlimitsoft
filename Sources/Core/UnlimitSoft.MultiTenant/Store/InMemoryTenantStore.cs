using System;
using System.Collections.Generic;
using System.Linq;

namespace UnlimitSoft.MultiTenant.Store;

/// <summary>
/// In memory store. If the tenant not found use the <see cref="String.Empty"/> value
/// </summary>
public sealed class InMemoryTenantStore : ITenantStore
{
    private readonly IDictionary<string, Tenant> _tenants;

    /// <summary>
    /// Initialize new instance.
    /// </summary>
    /// <param name="tenants"></param>
    public InMemoryTenantStore(IEnumerable<Tenant> tenants)
    {
        _tenants = tenants
            .Where(p => p.Key is not null)
            .ToDictionary(k => k.Key!);
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