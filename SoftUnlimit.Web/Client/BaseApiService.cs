using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Text;
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
            : this(apiClient, null, null)
        {
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
        protected object PrevCache { get; private set; }

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
        /// 
        /// </summary>
        protected CacheItemPolicy CacheItemPolicy { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        protected TResult GetDataFromCache<TResult>() where TResult : class
        {
            var cacheKey = typeof(TResult).FullName;
            return GetDataFromCache<TResult>(cacheKey);
        }
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
        /// <param name="data"></param>
        /// <returns></returns>
        protected TResult AddDataToCache<TResult>(TResult data) where TResult : class
        {
            var cacheKey = typeof(TResult).FullName;
            return AddDataToCache(cacheKey, data);
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
                        Cache.Add(cacheKey, data, CacheItemPolicy);
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
            var data = GetDataFromCache<TResult>();
            if (data != null)
                return data;

            try
            {
                var result = await func();
                return AddDataToCache(result);
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
