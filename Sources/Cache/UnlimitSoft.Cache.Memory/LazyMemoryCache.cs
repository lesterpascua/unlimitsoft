using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using static UnlimitSoft.Cache.ICache;

namespace UnlimitSoft.Cache.Memory;


/// <summary>
/// Implement in memory cache
/// </summary>
public sealed class LazyMemoryCache : ICache
{
    private CachingService _appCache;
    private readonly TimeSpan _expiration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expiration"></param>
    public LazyMemoryCache(TimeSpan? expiration = null)
    {
        _appCache = Create();
        _expiration = expiration ?? TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Gets the caching service used to store and retrieve application data in memory.
    /// </summary>
    public CachingService CachingService => _appCache;

    /// <inheritdoc />
    public ValueTask<bool> CleanAsync()
    {
        _appCache = Create();
        return new ValueTask<bool>(true);
    }
    /// <inheritdoc />
    public ValueTask<bool> RemoveAsync(string key)
    {
        _appCache.Remove(key);
#if NETSTANDARD
        return new ValueTask<bool>(true);
#else
        return ValueTask.FromResult(true);
#endif
    }
    /// <inheritdoc />
    public ValueTask<bool> ExistAsync(string key)
    {
        var exist = _appCache.Get<object>(key) is not null;
#if NETSTANDARD
        return new ValueTask<bool>(exist);
#else
        return ValueTask.FromResult(exist);
#endif
    }

    /// <inheritdoc />
    public ValueTask<TResult> GetAync<TResult>(string key)
    {
        var data = _appCache.Get<TResult>(key);
#if NETSTANDARD
        return new ValueTask<TResult>(data);
#else
        return ValueTask.FromResult(data);
#endif
    }
    /// <inheritdoc />
    public async ValueTask<TResult> GetOrCreateAsync<TResult>(string key, Operation<TResult> action, Setup? setup = null)
    {
        return await _appCache.GetOrAddAsync(key, entry =>
        {
            var config = new Config(key, entry) { AbsoluteExpirationRelativeToNow = _expiration };
            if (setup is not null && setup(ref config))
                entry.AbsoluteExpirationRelativeToNow = config.AbsoluteExpirationRelativeToNow;

            return action(key);
        });
    }

    #region Private Methods
    private static CachingService Create() => new (
        new Lazy<ICacheProvider>(() =>
        {
            var options = new MemoryCacheOptions();
            return new MemoryCacheProvider(new MemoryCache(Options.Create(options)));
        })
    );
    #endregion
}