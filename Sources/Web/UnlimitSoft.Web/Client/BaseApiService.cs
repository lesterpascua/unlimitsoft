using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public abstract class BaseApiService : IApiService, IDisposable
{
    private readonly IApiClient _apiClient;
    private readonly ICache? _cache;
    private readonly ConcurrentDictionary<object, object?>? _prevCache;
    private readonly ILogger? _logger;


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
        _logger = logger;
        if (!ignorePrevCache)
            _prevCache = new ConcurrentDictionary<object, object?>();
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
    /// If false save the previous value in the cache to in case the service is not available return this value.
    /// </summary>
    protected bool IgnorePrevCache => _prevCache is not null;
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
    /// <param name="factory"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    protected async ValueTask<TResult> TryCacheFirst<TResult>(Func<object, Task<TResult>> factory, object? key = null)
    {
        if (_cache is null)
            throw new InvalidOperationException("Cache is not set");

        var cacheKey = key ?? typeof(TResult).FullName!;
        if (_prevCache is null)
            return await _cache.GetOrCreateAsync(cacheKey, factory);

        try
        {
            return await _cache.GetOrCreateAsync(cacheKey, async cacheKey =>
            {
                var value = await factory(cacheKey);
                _prevCache[cacheKey] = value;
                return value;
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error retrieving cache");
            if (_prevCache.TryGetValue(cacheKey, out var value) && value is not null)
                return (TResult)value;

            throw;
        }
    }
}
