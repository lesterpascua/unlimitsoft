using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using UnlimitSoft.Distribute;
using UnlimitSoft.Json;
using static UnlimitSoft.Cache.ICache;

namespace UnlimitSoft.Cache.Redis;



/// <summary>
/// Implement in memory cache
/// </summary>
public sealed class RedisCache : ICache, IDisposable
{
    private readonly IDatabase _database;
    private readonly ConnectionMultiplexer _connection;
    private readonly string? _prefix;
    private readonly ISysLock _sysLock;
    private readonly IJsonSerializer _serializer;
    private readonly TimeSpan _defaultExpiration;
    private readonly ILogger<RedisCache> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hostName"></param>
    /// <param name="prefix"></param>
    /// <param name="sysLock"></param>
    /// <param name="serializer"></param>
    /// <param name="defaultExpiration"></param>
    /// <param name="logger"></param>
    public RedisCache(string hostName, string? prefix, ISysLock sysLock, IJsonSerializer serializer, TimeSpan defaultExpiration, ILogger<RedisCache> logger)
    {
        _connection = ConnectionMultiplexer.Connect(hostName);
        _database = _connection.GetDatabase();
        _prefix = prefix;
        _sysLock = sysLock;
        _serializer = serializer;
        _defaultExpiration = defaultExpiration;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask<bool> ExistAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }
    /// <inheritdoc />
    public async ValueTask<bool> RemoveAsync(string key)
    {
        await using var _ = await _sysLock.AcquireAsync(GetLock(key));
        return await _database.KeyDeleteAsync(key);
    }
    /// <inheritdoc />
    public async ValueTask<TResult> GetOrCreateAsync<TResult>(string key, Operation<TResult> action, Setup? setup = null)
    {
        if (_prefix is not null)
            key = $"{_prefix}_{key}";

        // Try to get data from cache
        var redisValueWithExpiry = await _database.StringGetWithExpiryAsync(key);
        var redisValue = redisValueWithExpiry.Value;

        if (!redisValue.IsNull && TryToGetDataFromCache<TResult>(redisValue, out var result))
            return result!;

        await using var _ = await _sysLock.AcquireAsync(GetLock(key));

        // Try to get data from cache again in a cretical section
        redisValueWithExpiry = await _database.StringGetWithExpiryAsync(key);
        redisValue = redisValueWithExpiry.Value;

        if (!redisValue.IsNull && TryToGetDataFromCache(redisValue, out result))
            return result!;
        var expity = GetAbsoluteExpirationRelativeToNow(key, setup);

        result = await action(key);

        if (result is null)
            return result;

        redisValue = _serializer.Serialize(result);
        var success = await _database.StringSetAsync(
            key,
            redisValue,
            expity,
            when: When.Always,
            flags: CommandFlags.None
        );
        if (!success)
            _logger.LogWarning("Error writing redis cache for {json}", redisValue);

        return result;
    }

    #region Private Methods
    /// <summary>
    /// Get key use to lock the operation
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string GetLock(string key) => $"lock_{key}";
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="setup"></param>
    /// <returns></returns>
    private TimeSpan? GetAbsoluteExpirationRelativeToNow(string key, Setup? setup)
    {
        var config = new Config(key, null) { AbsoluteExpirationRelativeToNow = _defaultExpiration };
        if (setup is not null && setup.Invoke(ref config))
            return config.AbsoluteExpirationRelativeToNow;
        return _defaultExpiration;
    }
    private bool TryToGetDataFromCache<TResult>(RedisValue redisValue, out TResult? value)
    {
        try
        {
            // If data is corrupted this cause an exception
            value = _serializer.Deserialize<TResult>(redisValue)!;
            return true;
        }
        catch { _logger.LogWarning("Redis data is corrupted {Json}", (string?)redisValue); }

        value = default;
        return false;
    }
    #endregion
}