using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoftUnlimit.MultiTenant.Store;

/// <summary>
/// In memory store. If the tenant not found use the <see cref="String.Empty"/> value
/// </summary>
public class InMemoryTenantStore<T> : ITenantStore<T> where T : Tenant
{
    private readonly IDictionary<string, T> _tenants;

    /// <summary>
    /// Initialize new instance.
    /// </summary>
    /// <param name="tenants"></param>
    public InMemoryTenantStore(IEnumerable<T> tenants)
    {
        _tenants = tenants.ToDictionary(k => k.Key);
    }

    /// <summary>
    /// Get a tenant for a given identifier
    /// </summary>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public ValueTask<T> GetTenantAsync(string identifier)
    {
        if (!_tenants.TryGetValue(identifier, out var tenant) && !_tenants.TryGetValue(string.Empty, out tenant))
            throw new KeyNotFoundException($"Key {identifier} not found");

        return new ValueTask<T>(tenant);
    }
}