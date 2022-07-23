using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace SoftUnlimit.MultiTenant.Options;

/// <summary>
/// Dictionary of tenant specific options caches
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public class TenantOptionsCacheDictionary<TOptions> where TOptions : class
{
    /// <summary>
    /// Caches stored in memory
    /// </summary>
    private readonly ConcurrentDictionary<Guid, IOptionsMonitorCache<TOptions>> _tenantSpecificOptionCaches = new();

    /// <summary>
    /// Get options for specific tenant (create if not exists)
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IOptionsMonitorCache<TOptions> Get(Guid tenantId) => _tenantSpecificOptionCaches.GetOrAdd(tenantId, new OptionsCache<TOptions>());
}