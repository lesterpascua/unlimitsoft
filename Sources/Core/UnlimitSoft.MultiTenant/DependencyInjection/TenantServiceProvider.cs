using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnlimitSoft.MultiTenant.DependencyInjection;


/// <summary>
/// Provider root used to create an specific provider per tenant.
/// </summary>
public sealed class TenantServiceProvider : IServiceProvider, IDisposable
{
    private readonly object _lock;
    private readonly IServiceProvider _root;                                            // root provider.
    private readonly ServiceDescriptor[] _descriptors;                                  // root service descriptos.
    private readonly Dictionary<Guid, IServiceProvider> _tenantProviders;               // Keeps track of all of the tenant scopes that we have created

    private readonly ITenantAccessService _tenantAccessService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="options"></param>
    public TenantServiceProvider(IServiceCollection services, ServiceProviderOptions options)
    {
        _lock = new();
        _tenantProviders = new();
        _descriptors = services.ToArray();
        _root = services.BuildServiceProvider(options);

        _tenantAccessService = _root.GetRequiredService<ITenantAccessService>();
    }

    /// <summary>
    /// 
    /// </summary>
    public IServiceProvider Root => _root;

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var tenant in _tenantProviders.Values)
            if (tenant is IDisposable tenantDisposble)
                tenantDisposble.Dispose();
        if (_root is IDisposable disposable)
            disposable.Dispose();

        GC.SuppressFinalize(this);
    }
    /// <summary>
    /// Remove the provider for a tenant so the next time the system will recreate
    /// </summary>
    /// <param name="tenant"></param>
    public void Clean(Tenant tenant)
    {
        IServiceProvider? tenantProvider = null;
        if (_tenantProviders.ContainsKey(tenant.Id))
            lock (_lock)
                if (_tenantProviders.TryGetValue(tenant.Id, out tenantProvider))
                    _tenantProviders.Remove(tenant.Id);
        if (tenantProvider is IDisposable disposable)
            disposable.Dispose();
    }
    /// <inheritdoc />
    public object? GetService(Type serviceType) => GetCurrentProvider().GetService(serviceType);

    #region Private Methods
    /// <summary>
    /// Get the current teanant from the application container
    /// </summary>
    /// <returns></returns>
    private Tenant? GetCurrentTenant()
    {
        // We have registered our TenantAccessService in Part 1, the service is
        // available in the application container which allows us to access the current Tenant
        return _tenantAccessService.GetTenant();
    }
    /// <summary>
    /// Get the scope of the current tenant
    /// </summary>
    /// <returns></returns>
    private IServiceProvider GetCurrentProvider()
    {
        var tenant = GetCurrentTenant();

        // If no tenant (e.g. early on in the pipeline, we just use the application container)
        if (tenant is null)
            return _root;

        var tenantId = tenant.Id;

        // If we have created a lifetime for a tenant, return
        if (_tenantProviders.TryGetValue(tenantId, out var provider))
            return provider;

        lock (_lock)
        {
            if (_tenantProviders.TryGetValue(tenantId, out provider))
                return provider;
            
            var tenantProvider = BuildTenantProvider(tenant);

            var tenantConfigure = tenantProvider.GetService<ITenantConfigure>();
            tenantConfigure?.Configure(tenant);

            // This is a new tenant, configure a new LifeTimeScope for it using our tenant sensitive configuration method
            _tenantProviders.Add(tenantId, tenantProvider);

            return tenantProvider;
        }
    }
    /// <summary>
    /// Build service provider. When a tenant provider is create this will inherit all service descriptor in the root provider, this is 
    /// usefull to keep the reference of the instance already registers.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private IServiceProvider BuildTenantProvider(Tenant tenant)
    {
        var services = new ServiceCollection();
        for (var i = 0; i < _descriptors.Length; i++)
            services.Add(CreateDescriptor(_descriptors[i]));
        services.AddSingleton<ITenantProviderReset>(p => new TenantProviderReset(tenant, this));

        var setup = _root.GetService<ITenantConfigureServices>();
        setup?.ConfigureTenantServices(tenant, services);

        return services.BuildServiceProvider();
    }
    /// <summary>
    /// Get service descriptor taken from the root provider.
    /// </summary>
    /// <param name="registry"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    private ServiceDescriptor CreateDescriptor(ServiceDescriptor registry)
    {
        if (registry.ImplementationType is not null)
        {
            if (registry.Lifetime == ServiceLifetime.Scoped || registry.ImplementationType.IsGenericTypeDefinition)
                return new ServiceDescriptor(registry.ServiceType, registry.ImplementationType, registry.Lifetime);

            //if (registry.Lifetime == ServiceLifetime.Scoped)
            //    return new ServiceDescriptor(registry.ServiceType, registry.ImplementationType, registry.Lifetime);

            return new ServiceDescriptor(registry.ServiceType, _ => _root.GetRequiredService(registry.ServiceType), registry.Lifetime);
        }

        if (registry.ImplementationInstance is not null)
            return new ServiceDescriptor(registry.ServiceType, _ => registry.ImplementationInstance, registry.Lifetime);

        if (registry.ImplementationFactory is not null)
            return registry.Lifetime switch
            {
                ServiceLifetime.Singleton => new ServiceDescriptor(registry.ServiceType, _ => registry.ImplementationFactory(_root), registry.Lifetime),
                ServiceLifetime.Scoped or ServiceLifetime.Transient => new ServiceDescriptor(registry.ServiceType, provider => registry.ImplementationFactory(provider), registry.Lifetime),
                _ => throw new NotSupportedException("Never happend")
            };

        throw new NotSupportedException("Invalid descriptor");
    }
    #endregion
}