using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using UnlimitSoft.MultiTenant.Options;
using UnlimitSoft.MultiTenant.ResolutionStrategy;
using UnlimitSoft.MultiTenant.Store;
using System;
using System.Collections.Generic;

namespace UnlimitSoft.MultiTenant.DependencyInjection;


/// <summary>
/// Configure tenant services
/// </summary>
public sealed class TenantBuilder
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
        _services.TryAddSingleton<ITenantAccessService, TenantAccessService>();
    }

    /// <summary>
    /// 
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Register the tenant resolver implementation
    /// </summary>
    /// <remarks>
    /// Register: <see cref="ITenantResolutionStrategy"/> and <see cref="ITenantAccessService"/>
    /// </remarks>
    /// <typeparam name="V"></typeparam>
    /// <returns></returns>
    public TenantBuilder WithResolutionStrategy<V>() where V : class, ITenantResolutionStrategy
    {
        _services.AddSingleton<ITenantResolutionStrategy, V>();

        return this;
    }
    /// <summary>
    /// Register the tenant resolver implementation
    /// </summary>
    /// <remarks>
    /// Register: <see cref="ITenantResolutionStrategy"/> and <see cref="ITenantAccessService"/>
    /// </remarks>
    /// <typeparam name="V"></typeparam>
    /// <returns></returns>
    public TenantBuilder WithResolutionStrategy<V>(Func<IServiceProvider, V> implementationFactory) where V : class, ITenantResolutionStrategy
    {
        _services.AddSingleton<ITenantResolutionStrategy>(implementationFactory);

        return this;
    }

    /// <summary>
    /// Register the tenant store implementation.
    /// </summary>
    /// <remarks>
    /// Store is the place where all tenant will be resolve base of some string identifier.
    /// </remarks>
    /// <typeparam name="V">Store type to register.</typeparam>
    /// <returns></returns>
    public TenantBuilder WithStore<V>() where V : class, ITenantStore
    {
        _services.AddSingleton<ITenantStore, V>();
        return this;
    }
    /// <summary>
    /// Register the tenant store implementation.
    /// </summary>
    /// <remarks>
    /// Store is the place where all tenant will be resolve base of some string identifier.
    /// </remarks>
    /// <typeparam name="V">Store type to register.</typeparam>
    /// <returns></returns>
    public TenantBuilder WithStore<V>(Func<IServiceProvider, V> implementationFactory) where V : class, ITenantStore
    {
        _services.AddSingleton<ITenantStore>(implementationFactory);
        return this;
    }
    /// <summary>
    /// </summary>
    /// <param name="tenants"></param>
    /// <returns></returns>
    public TenantBuilder WithMemoryStore(IEnumerable<Tenant> tenants)
    {
        _services.AddSingleton<ITenantStore>(_ => new InMemoryTenantStore(tenants));
        return this;
    }
    /// <summary>
    /// Register tenant specific options. This allow load global configuration for your tenant and load from the source you want.
    /// </summary>
    /// <example>
    /// If the tenant configuration for this service is store in a cloud use this method to get the connector, make the call, retrieve the configuration ad populate the options object. 
    /// </example>
    /// <remarks>Register the multi-tenant cache, options factory, IOptionsSnapshot, IOptions</remarks>
    /// <typeparam name="TOptions">Type of options we are apply configuration to</typeparam>
    /// <param name="timeLive">Tenant config cache</param>
    /// <param name="setup">Action to configure options for a tenant</param>
    /// <returns></returns>
    public TenantBuilder WithTenantConfigure<TOptions>(TimeSpan timeLive, Action<IServiceProvider, Tenant, TOptions> setup) where TOptions : class, new()
    {
        _services.AddSingleton<IOptionsMonitorCache<TOptions>>(p => ActivatorUtilities.CreateInstance<TenantOptionsCache<TOptions>>(p, timeLive));
        _services.AddTransient<IOptionsFactory<TOptions>>(provider =>
        {
            Action<Tenant, TOptions> inner = (o, t) => setup(provider, o, t);
            return ActivatorUtilities.CreateInstance<TenantOptionsFactory<TOptions>>(provider, inner);
        });

        _services.AddScoped<IOptionsSnapshot<TOptions>>(p => ActivatorUtilities.CreateInstance<TenantOptions<TOptions>>(p));
        _services.AddSingleton<IOptions<TOptions>>(p => ActivatorUtilities.CreateInstance<TenantOptions<TOptions>>(p));

        return this;
    }
}