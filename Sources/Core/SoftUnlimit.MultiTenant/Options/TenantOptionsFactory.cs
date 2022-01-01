using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace SoftUnlimit.MultiTenant.Options;

/// <summary>
/// Create a new options instance with configuration applied
/// </summary>
/// <typeparam name="TOptions"></typeparam>
/// <typeparam name="T"></typeparam>
internal class TenantOptionsFactory<TOptions, T> : IOptionsFactory<TOptions>
    where TOptions : class, new()
    where T : Tenant
{
    private readonly Action<TOptions, T> _setup;
    private readonly ITenantAccessService<T> _tenantService;
    private readonly IEnumerable<IConfigureOptions<TOptions>> _setups;
    private readonly IEnumerable<IPostConfigureOptions<TOptions>> _postConfigures;
    private readonly ILogger<TenantOptionsFactory<TOptions, T>> _logger;

    public TenantOptionsFactory(
        Action<TOptions, T> setup,
        ITenantAccessService<T> tenantService,
        IEnumerable<IConfigureOptions<TOptions>> setups,
        IEnumerable<IPostConfigureOptions<TOptions>> postConfigures,
        ILogger<TenantOptionsFactory<TOptions, T>> logger
    )
    {
        _setups = setups;
        _postConfigures = postConfigures;
        _logger = logger;
        _tenantService = tenantService;
        _setup = setup;
    }

    /// <summary>
    /// Create a new options instance
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public TOptions Create(string name)
    {
        var options = new TOptions();

        // Apply options setup configuration
        foreach (var setup in _setups)
            Configure(name, options, setup);

        // Apply tenant specifc configuration (to both named and non-named options)
        var tenant = _tenantService.GetTenant();
        if (tenant is not null)
            _setup(options, tenant);

        _logger.LogInformation("Create {Options} for {Tenant}", typeof(TOptions).Name, tenant?.Id);

        // Apply post configuration
        foreach (var postConfig in _postConfigures)
            postConfig.PostConfigure(name, options);

        return options;
    }

    private static void Configure(string name, TOptions options, IConfigureOptions<TOptions> setup)
    {
        if (setup is IConfigureNamedOptions<TOptions> namedSetup)
        {
            namedSetup.Configure(name, options);
            return;
        }
        setup.Configure(options);
    }
}