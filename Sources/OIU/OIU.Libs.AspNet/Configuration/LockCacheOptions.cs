using OIU.Libs.Distribute;

namespace OIU.Libs.AspNet.Configuration;


/// <summary>
/// 
/// </summary>
public sealed class LockCacheOptions
{
    /// <summary>
    /// Configuration over lock
    /// </summary>
    public LockSection? Lock { get; set; }
    /// <summary>
    /// Configuration over the cache
    /// </summary>
    public CacheSection? Cache { get; set; }

    #region Nested Classes
    /// <summary>
    /// 
    /// </summary>
    public sealed class LockSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool UseEnvAsPrefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SysType SysType { get; set; }
        /// <summary>
        /// If redis cache is enable this value should not null
        /// </summary>
        public RedisSection? Redis { get; set; }
        /// <summary>
        /// If Sql cache is enable this value should not null
        /// </summary>
        public SqlSection? Sql { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class CacheSection
    {
        /// <summary>
        /// 
        /// </summary>
        public bool UseEnvAsPrefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SysType SysType { get; set; } = SysType.Sql;
        /// <summary>
        /// If SQL cache is enable this value should not null
        /// </summary>
        public SqlSection? Sql { get; set; }
        /// <summary>
        /// If redis cache is enable this value should not null
        /// </summary>
        public RedisSection? Redis { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class SqlSection
    {
        /// <summary>
        /// Connection string to the server using to make the distribute lock if this value is null will take the value from the connection string.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class RedisSection
    {
        /// <summary>
        /// Redis host or ip 
        /// </summary>
        public string HostName { get; set; } = default!;
    }
    #endregion
}
