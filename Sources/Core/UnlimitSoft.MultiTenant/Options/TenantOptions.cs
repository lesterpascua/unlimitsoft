using Microsoft.Extensions.Options;
using System.Threading;

namespace UnlimitSoft.MultiTenant.Options;


/// <summary>
/// Make IOptions tenant aware
/// </summary>
public sealed class TenantOptions<TOptions> : IOptions<TOptions>, IOptionsSnapshot<TOptions> where TOptions : class, new()
{
    private TOptions? _options;
    private readonly IOptionsFactory<TOptions> _factory;
    private readonly IOptionsMonitorCache<TOptions> _cache;

    /// <summary>
    /// Initialize new instance.
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="cache"></param>
    public TenantOptions(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
    {
        _factory = factory;
        _cache = cache;
    }

    /// <inheritdoc />
    public TOptions Value => Get(Microsoft.Extensions.Options.Options.DefaultName);

    /// <inheritdoc />
    public TOptions Get(string? name)
    {
        if (_options is null)
            Interlocked.CompareExchange(ref _options, _cache.GetOrAdd(name, () => _factory.Create(name)), null);
        return _options;
    }
}