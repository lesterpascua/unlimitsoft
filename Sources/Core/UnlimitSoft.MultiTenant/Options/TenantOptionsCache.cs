using Microsoft.Extensions.Options;
using System;

namespace UnlimitSoft.MultiTenant.Options;

/// <summary>
/// Tenant aware options cache
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public sealed class TenantOptionsCache<TOptions> : IOptionsMonitorCache<TOptions>
    where TOptions : class
{
    private readonly TimeSpan _timeLive;
    private readonly ITenantAccessService _tenantAccessor;
    private readonly TenantOptionsCacheDictionary<TOptions> _tenantSpecificOptionsCache;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenantAccessor"></param>
    /// <param name="timeLive"></param>
    public TenantOptionsCache(ITenantAccessService tenantAccessor, TimeSpan timeLive)
    {
        _timeLive = timeLive;
        _tenantAccessor = tenantAccessor;
        _tenantSpecificOptionsCache = new();
    }

    /// <inheritdoc />
    public void Clear()
    {
        var tenant = GetTenant();
        if (tenant is not null)
            _tenantSpecificOptionsCache.Get(tenant.Id).Clear();
    }
    /// <inheritdoc />
    public TOptions GetOrAdd(string? name, Func<TOptions> createOptions)
    {
        var tenant = GetTenant();
        if (tenant is null)
            return createOptions();

        return _tenantSpecificOptionsCache.Get(tenant.Id).GetOrAdd(name, createOptions);
    }
    /// <inheritdoc />
    public bool TryAdd(string? name, TOptions options)
    {
        var tenant = GetTenant();
        if (tenant is not null)
            return _tenantSpecificOptionsCache.Get(tenant.Id).TryAdd(name, options);

        return false;
    }
    /// <inheritdoc />
    public bool TryRemove(string? name)
    {
        var tenant = GetTenant();
        if (tenant is not null)
            return _tenantSpecificOptionsCache.Get(tenant.Id).TryRemove(name);

        return false;
    }

    private Tenant? GetTenant() => _tenantAccessor.GetTenant();
}
