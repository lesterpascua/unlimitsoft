using System;
using System.Threading.Tasks;

namespace UnlimitSoft.Web.Client;



/// <summary>
/// 
/// </summary>
public interface ICache
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="key"></param>
    /// <param name="setup"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    ValueTask<TResult> GetOrCreateAsync<TResult>(string key, Operation<TResult> action, Setup? setup = null);

    #region Nested Classes
    /// <summary>
    /// Cache entry configuration
    /// </summary>
    public struct Config
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheEntryObject"></param>
        public Config(string key, object? cacheEntryObject)
        {
            Key = key;
            CacheEntryObject = cacheEntryObject;
        }
        /// <summary>
        /// Key used to access the cache
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Original cache parameter from the specific platform used
        /// </summary>
        public object? CacheEntryObject { get; }
        /// <summary>
        /// Time of the cache expiration relative from the current date.
        /// </summary>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
    }
    /// <summary>
    /// Allow configure the cache information like life time, etc.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public delegate bool Setup(ref Config config);
    /// <summary>
    /// Function to resolve the value and also indicate the cache time
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public delegate Task<TResult> Operation<TResult>(string key);
    #endregion
}
