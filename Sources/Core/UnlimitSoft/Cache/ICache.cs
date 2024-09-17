using System;
using System.Threading.Tasks;

namespace UnlimitSoft.Cache;



/// <summary>
/// 
/// </summary>
public interface ICache
{
    /// <summary>
    /// Check if the key exists in the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    ValueTask<bool> ExistAsync(string key);
    /// <summary>
    /// Remove existing value from the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    ValueTask<bool> RemoveAsync(string key);
    /// <summary>
    /// Get the value from the cache if not found will execute the action to get the value
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="key">Key to represent the cache</param>
    /// <param name="action">Deletage to return the data from the source</param>
    /// <param name="setup">Method to configure the time to live of the cache</param>
    /// <returns></returns>
    ValueTask<TResult> GetOrCreateAsync<TResult>(string key, Operation<TResult> action, Setup? setup = null);

    #region Nested Classes
    /// <summary>
    /// Cache entry configuration
    /// </summary>
    public ref struct Config
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
    /// <returns>True indicate to take the data from the configuration, false to ignore.</returns>
    public delegate bool Setup(ref Config config);
    /// <summary>
    /// Function to resolve the value and also indicate the cache time
    /// </summary>
    /// <typeparam name="TResult">Type of the data to be cache</typeparam>
    /// <returns>Data to be cache</returns>
    public delegate Task<TResult> Operation<TResult>(string key);
    #endregion
}
/// <summary>
/// 
/// </summary>
public interface IAdvanceCache : ICache
{
    /// <summary>
    /// Get how many time the key will be available in the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    ValueTask<TimeSpan> GetRemaindTimeAsync(string key);
}