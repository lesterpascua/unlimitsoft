using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public abstract class BaseApiService : IApiService, IDisposable
{
    private readonly IApiClient _apiClient;
    private readonly ICache? _cache;
    private readonly bool _ignorePrevCache;
    private readonly ILogger? _logger;
    private static ConcurrentDictionary<string, object?>? _prevCache;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiClient"></param>
    /// <param name="cache"></param>
    /// <param name="ignorePrevCache"></param>
    /// <param name="logger"></param>
    protected BaseApiService(IApiClient apiClient, ICache? cache = null, bool ignorePrevCache = false, ILogger? logger = null)
    {
        _apiClient = apiClient;
        _cache = cache;
        _ignorePrevCache = ignorePrevCache;
        _logger = logger;
    }

    /// <summary>
    /// Get the prev cache in memory object this consume more resource but warantine alwasy a result after the first attempt
    /// </summary>
    protected static ConcurrentDictionary<string, object?> PrevCache
    {
        get
        {
            if (_prevCache is not null)
                return _prevCache;
            Interlocked.CompareExchange(ref _prevCache, new ConcurrentDictionary<string, object?>(), null);
            return _prevCache;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    protected ICache? Cache => _cache;
    /// <summary>
    /// 
    /// </summary>
    protected IApiClient ApiClient => _apiClient;
    /// <summary>
    /// Logger used in the service
    /// </summary>
    protected ILogger? Logger => _logger;

    /// <inheritdoc />
    public void Dispose()
    {
        _apiClient.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Try to get data from cache if not exist execute function and update cache object.
    /// </summary>
    /// <remarks>
    /// If the endpoint is not available and the cache was load in some time alwais return the old cache data.
    /// </remarks>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="action"></param>
    /// <param name="setup">action used to setup the cache arguments</param>
    /// <param name="key"></param>
    /// <returns></returns>
    protected ValueTask<TResult> TryCacheFirst<TResult>(ICache.Operation<TResult> action, ICache.Setup? setup = null, string? key = null)
    {
        if (_cache is null)
            throw new InvalidOperationException("Cache is not set");

        key ??= typeof(TResult).FullName!;
        if (_ignorePrevCache)
            return _cache.GetOrCreateAsync(key, action, setup);

        try
        {
            return _cache.GetOrCreateAsync(
                key,
                (k) => action(k).ContinueWith(c =>
                {
                    PrevCache[k] = c.Result;
                    return c.Result;
                }),
                setup
            );
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error retrieving cache");
            if (PrevCache.TryGetValue(key, out var value) && value is not null)
#if NETSTANDARD
                return new ValueTask<TResult>((TResult)value);
#else
                return ValueTask.FromResult((TResult)value);
#endif

            throw;
        }
    }
}
