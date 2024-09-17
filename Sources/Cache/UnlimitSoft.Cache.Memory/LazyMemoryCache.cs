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
    private readonly CachingService _appCache;
    private readonly TimeSpan _expiration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="expiration"></param>
    public LazyMemoryCache(TimeSpan? expiration = null)
    {
        _appCache = new CachingService(
            new Lazy<ICacheProvider>(() =>
            {
                var options = new MemoryCacheOptions();
                var cache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(options));
                return new MemoryCacheProvider(cache);
            })
        );
        _expiration = expiration ?? TimeSpan.FromHours(1);
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
}