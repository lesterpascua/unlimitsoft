using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SoftUnlimit.MultiTenant.Options;
using SoftUnlimit.MultiTenant.ResolutionStrategy;
using SoftUnlimit.MultiTenant.Store;
using System;
using System.Collections.Generic;

namespace SoftUnlimit.MultiTenant.DependencyInjection;

/// <summary>
/// Configure tenant services
/// </summary>
public class TenantBuilder<T> where T : Tenant
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Build configuration for multi tenant
    /// </summary>
    /// <param name="services"></param>
    public TenantBuilder(IServiceCollection services)
    {
        _services = services;
        _services.TryAddSingleton<ITenantContextAccessor, TenantContextAccessor>();
    }

    /// <summary>
    /// 
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Register the tenant resolver implementation
    /// </summary>
    /// <remarks>
    /// Register: <see cref="ITenantResolutionStrategy"/> and <see cref="ITenantAccessService{T}"/>
    /// </remarks>
    /// <typeparam name="V"></typeparam>
    /// <returns></returns>
    public TenantBuilder<T> WithResolutionStrategy<V>() where V : class, ITenantResolutionStrategy
    {
        _services.AddSingleton<ITenantResolutionStrategy, V>();
        _services.AddSingleton<ITenantAccessService<T>, TenantAccessService<T>>();

        return this;
    }
    /// <summary>
    /// Register the tenant store implementation
    /// </summary>
    /// <remarks>
    /// Register: <see cref="ITenantStore{V}"/>
    /// </remarks>
    /// <typeparam name="V">Store type to register.</typeparam>
    /// <returns></returns>
    public TenantBuilder<T> WithStore<V>() where V : class, ITenantStore<T>
    {
        _services.AddSingleton<ITenantStore<T>, V>();
        return this;
    }
    /// <summary>
    /// Register the tenant <see cref="InMemoryTenantStore{T}"/>.
    /// </summary>
    /// <param name="tenants"></param>
    /// <returns></returns>
    public TenantBuilder<T> WithMemoryStore(IEnumerable<T> tenants)
    {
        _services.AddSingleton<ITenantStore<T>>(_ => new InMemoryTenantStore<T>(tenants));
        return this;
    }
    /// <summary>
    /// Register tenant specific options
    /// </summary>
    /// <remarks>Register the multi-tenant cache, options factory, IOptionsSnapshot, IOptions</remarks>
    /// <typeparam name="TOptions">Type of options we are apply configuration to</typeparam>
    /// <param name="timeLive">Tenant config cache</param>
    /// <param name="setup">Action to configure options for a tenant</param>
    /// <returns></returns>
    public TenantBuilder<T> WithTenantConfigure<TOptions>(TimeSpan timeLive, Action<IServiceProvider, TOptions, T> setup) where TOptions : class, new()
    {
        _services.AddSingleton<IOptionsMonitorCache<TOptions>>(p => ActivatorUtilities.CreateInstance<TenantOptionsCache<TOptions, T>>(p, timeLive));
        _services.AddTransient<IOptionsFactory<TOptions>>(provider =>
        {
            Action<TOptions, T> inner = (o, t) => setup(provider, o, t);
            return ActivatorUtilities.CreateInstance<TenantOptionsFactory<TOptions, T>>(provider, inner);
        });

        _services.AddScoped<IOptionsSnapshot<TOptions>>(p => ActivatorUtilities.CreateInstance<TenantOptions<TOptions>>(p));
        _services.AddSingleton<IOptions<TOptions>>(p => ActivatorUtilities.CreateInstance<TenantOptions<TOptions>>(p));

        return this;
    }
}