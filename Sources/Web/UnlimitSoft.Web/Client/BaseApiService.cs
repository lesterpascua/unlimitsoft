using Microsoft.Extensions.Logging;
using System;
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
    protected bool IgnorePrevCache => _ignorePrevCache;
    /// <summary>
    /// Logger used in the service
    /// </summary>
    protected ILogger? Logger => _logger;
    /// <summary>
    /// Old cache value.
    /// </summary>
    protected object? PrevCache { get; set; }


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
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="factory"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    protected async ValueTask<TResult> TryCacheFirst<TResult, TEntry>(Func<TEntry, Task<TResult>> factory, string? key = null)
    {
        var cacheKey = key ?? typeof(TResult).FullName!;
        if (_ignorePrevCache)
            return await _cache!.GetOrCreateAsync(cacheKey, factory);

        try
        {
            return await _cache!.GetOrCreateAsync<TResult, TEntry>(cacheKey, async entry =>
            {
                var aux = await factory(entry);
                PrevCache = aux;
                return aux;
            });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error retrieving cache");
            if (PrevCache is not null)
                return (TResult)PrevCache;

            throw;
        }
    }
}
