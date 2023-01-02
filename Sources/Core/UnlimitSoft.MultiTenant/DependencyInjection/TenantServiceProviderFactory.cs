using Microsoft.Extensions.DependencyInjection;
using System;

namespace UnlimitSoft.MultiTenant.DependencyInjection;

/// <summary>
/// Build a provider using multitenance.
/// </summary>
public class TenantServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly ServiceProviderOptions _options;
    public static IServiceProvider? Root;

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
        TenantServiceProvider? provider = null;

        var providerAccessor = () => provider;
        containerBuilder.AddSingleton<Func<TenantServiceProvider>>(providerAccessor);

        Root = provider = new TenantServiceProvider(containerBuilder, _options);
        return provider;
    }
}
