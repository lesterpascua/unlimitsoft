using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    private const string LOCK_PREFIX = "__lock__";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hostName"></param>
    /// <param name="db">DB index to be used, by default -1</param>
    /// <param name="prefix"></param>
    /// <param name="sysLock"></param>
    /// <param name="serializer"></param>
    /// <param name="defaultExpiration"></param>
    /// <param name="logger"></param>
    public RedisCache(string hostName, int db, string? prefix, ISysLock sysLock, IJsonSerializer serializer, TimeSpan defaultExpiration, ILogger<RedisCache> logger)
    {
        _connection = ConnectionMultiplexer.Connect(hostName);
        _database = _connection.GetDatabase(db);
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

    /// <summary>
    /// Gets the database connection used by the current instance.
    /// </summary>
    public IDatabase Database => _database;
    /// <summary>
    /// Gets the underlying connection multiplexer used to communicate with the Redis server.
    /// </summary>
    /// <remarks>The returned connection multiplexer manages all network communication with the Redis server
    /// and can be used to perform advanced operations or access low-level features not exposed by higher-level
    /// abstractions. The caller should not dispose of the returned connection.</remarks>
    public IConnectionMultiplexer Connection => _connection;

    /// <summary>
    /// Asynchronously removes all keys from the Redis database that are not lock keys and match the configured prefix,
    /// if any.
    /// </summary>
    /// <remarks>This method scans all endpoints in the Redis connection and deletes keys that are not lock
    /// keys and, if a prefix is configured, only those that match the prefix. Lock keys and keys that do not match the
    /// prefix are excluded from deletion. The operation may take longer to complete if the database contains a large
    /// number of keys.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if any keys were
    /// removed; otherwise, <see langword="false"/>.</returns>
    public async ValueTask<bool> CleanAsync()
    {
        var found = false;
        foreach (var endpoint in _connection.GetEndPoints())
        {
            RedisKey[] keys;
            var server = _connection.GetServer(endpoint);
            do
            {
                keys = [.. server.Keys(database: _database.Database, pattern: "*", pageSize: 1000)
                    .Where(x => {
                        var key = x.ToString();
                        if (key.StartsWith(LOCK_PREFIX))
                            return false;
                        if (_prefix is not null && !key.StartsWith(_prefix))
                            return false;
                        if (key.StartsWith($"{LOCK_PREFIX}{_prefix}"))
                            return false;
                        return true;
                    })
                ];
                foreach (var entry in keys)
                {
                    var key = entry.ToString();
                    if (_prefix is not null)
                        key = $"{_prefix}_{key}";
                    if (key is null)
                        continue;

                    found = true;
                    await RemoveAsync(key);
                }
            } while (keys.Length != 0);
        }
        return found;
    }
    /// <inheritdoc />
    public async ValueTask<bool> ExistAsync(string key)
    {
        if (_prefix is not null)
            key = $"{_prefix}_{key}";
        return await _database.KeyExistsAsync(key);
    }
    /// <inheritdoc />
    public async ValueTask<bool> RemoveAsync(string key)
    {
        if (_prefix is not null)
            key = $"{_prefix}_{key}";
        await using var _ = await _sysLock.AcquireAsync(GetLock(key));
        return await _database.KeyDeleteAsync(key);
    }
    /// <inheritdoc />
    public async ValueTask<TResult> GetAync<TResult>(string key)
    {
        if (_prefix is not null)
            key = $"{_prefix}_{key}";

        // Try to get data from cache
        var redisValueWithExpiry = await _database.StringGetWithExpiryAsync(key);
        var redisValue = redisValueWithExpiry.Value;

        if (!redisValue.IsNull && TryToGetDataFromCache<TResult>(redisValue, out var result))
            return result;

        return default!;
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
    private static string GetLock(string key) => $"{LOCK_PREFIX}{key}";
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
    private bool TryToGetDataFromCache<TResult>(RedisValue redisValue, [NotNullWhen(true)] out TResult? value)
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