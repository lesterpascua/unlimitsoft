using System;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace SoftUnlimit.Web.Client
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class BaseApiService : IApiService, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiClient"></param>
        protected BaseApiService(IApiClient apiClient)
        {
            ApiClient = apiClient;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="cache"></param>
        /// <param name="slidingExpiration"></param>
        /// <param name="ignorePrevCache"></param>
        protected BaseApiService(IApiClient apiClient, ObjectCache cache, TimeSpan? slidingExpiration, bool ignorePrevCache = false)
        {
            ApiClient = apiClient;
            Cache = cache;
            SlidingExpiration = slidingExpiration;
            IgnorePrevCache = ignorePrevCache;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiClient"></param>
        /// <param name="cache"></param>
        /// <param name="cacheItemPolicy"></param>
        /// <param name="ignorePrevCache"></param>
        protected BaseApiService(IApiClient apiClient, ObjectCache cache, CacheItemPolicy cacheItemPolicy, bool ignorePrevCache = false)
        {
            ApiClient = apiClient;
            Cache = cache;
            CacheItemPolicy = cacheItemPolicy;
            if (!ignorePrevCache && CacheItemPolicy != null)
                CacheItemPolicy.RemovedCallback = (arguments) =>
                {
                    if (arguments.RemovedReason == CacheEntryRemovedReason.Expired)
                        PrevCache = arguments.CacheItem.Value;
                };
        }

        /// <summary>
        /// Old cache value.
        /// </summary>
        protected static object PrevCache { get; private set; }

        /// <inheritdoc />
        public void Dispose() => ApiClient.Dispose();

        /// <summary>
        /// 
        /// </summary>
        protected IApiClient ApiClient { get; }
        /// <summary>
        /// 
        /// </summary>
        protected ObjectCache Cache { get; }
        /// <summary>
        /// If false save the previous value in the cache to in case the service is not available return this value.
        /// </summary>
        protected bool IgnorePrevCache { get; }
        /// <summary>
        /// Create a cache to expire in some time inmidliatly after a valid request. Can't set if <see cref="CacheItemPolicy"/> is not null.
        /// </summary>
        protected TimeSpan? SlidingExpiration { get; }
        /// <summary>
        /// Cache item policy to use by default. Can't set if <see cref="SlidingExpiration"/> is not null.
        /// </summary>
        protected CacheItemPolicy CacheItemPolicy { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        protected TResult GetDataFromCache<TResult>(string cacheKey) where TResult : class
        {
            if (Cache?.Contains(cacheKey) == true)
                return Cache.Get(cacheKey) as TResult;

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected TResult AddDataToCache<TResult>(string cacheKey, TResult data) where TResult : class
        {
            if (Cache?.Contains(cacheKey) == false)
                lock (Cache)
                    if (!Cache.Contains(cacheKey))
                    {
                        CacheItemPolicy policy;
                        if (SlidingExpiration.HasValue || CacheItemPolicy is not null)
                        {
                            policy = CacheItemPolicy ?? new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.Add(SlidingExpiration.Value) };
                        }
                        else
                            policy = null;

                        if (!IgnorePrevCache && policy is not null)
                        {
                            CacheItemPolicy.RemovedCallback = (arguments) =>
                            {
                                if (arguments.RemovedReason == CacheEntryRemovedReason.Expired)
                                    PrevCache = arguments.CacheItem.Value;
                            };
                        }

                        Cache.Add(cacheKey, data, policy);
                    }
            return data;
        }

        /// <summary>
        /// Try to get data from cache if not exist execute function and update cache object.
        /// </summary>
        /// <remarks>
        /// If the endpoint is not available and the cache was load in some time alwais return the old cache data.
        /// </remarks>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        protected async Task<TResult> TryCacheFirst<TResult>(Func<Task<TResult>> func) where TResult : class
        {
            var cacheKey = typeof(TResult).FullName;

            var data = GetDataFromCache<TResult>(cacheKey);
            if (data != null)
                return data;

            try
            {
                var result = await func();
                return AddDataToCache(cacheKey, result);
            }
            catch
            {
                if (PrevCache != null)
                    return (TResult)PrevCache;

                throw;
            }
        }
    }
}
