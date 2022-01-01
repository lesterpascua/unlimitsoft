using Microsoft.Extensions.DependencyInjection;
using System;

namespace SoftUnlimit.MultiTenant.DependencyInjection;

/// <summary>
/// Build a provider using multitenance.
/// </summary>
/// <typeparam name="T"></typeparam>
public class TenantServiceProviderFactory<T> : IServiceProviderFactory<IServiceCollection> where T : Tenant
{
    private readonly ServiceProviderOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultServiceProviderFactory"/> class
    /// with default options.
    /// </summary>
    public TenantServiceProviderFactory() : this(new ServiceProviderOptions())
    {
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultServiceProviderFactory"/> class
    /// with the specified <paramref name="options"/>.
    /// </summary>
    /// <param name="options">The options to use for this instance.</param>
    public TenantServiceProviderFactory(ServiceProviderOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public IServiceCollection CreateBuilder(IServiceCollection services) => services;
    /// <inheritdoc />
    public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
    {
        TenantServiceProvider<T>? provider = null;

        var providerAccessor = () => provider;
        containerBuilder.AddSingleton(providerAccessor);

        provider = new TenantServiceProvider<T>(containerBuilder, _options);
        return provider;
    }
}
