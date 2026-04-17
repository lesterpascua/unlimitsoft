using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace OIU.Libs.Data.SQLServer;


/// <summary>
/// 
/// </summary>
public static class SqlServerInitHelper
{
    /// <summary>
    /// Default configuration for SQL server
    /// </summary>
    /// <param name="oBuilder"></param>
    /// <param name="maxRetryCount"></param>
    /// <param name="maxRetryDelay"></param>
    /// <param name="migration"></param>
    private static void ServerOptionsAction(SqlServerDbContextOptionsBuilder oBuilder, int maxRetryCount, int maxRetryDelay, Assembly? migration = null)
    {
        if (migration is not null)
            oBuilder.MigrationsAssembly(migration.FullName);

        oBuilder.EnableRetryOnFailure(maxRetryCount, TimeSpan.FromSeconds(maxRetryDelay), []);
    }
    /// <summary>
    /// Build reader UnitOfWork
    /// </summary>
    /// <param name="connString"></param>
    /// <param name="options"></param>
    /// <param name="maxRetryCount"></param>
    /// <param name="maxRetryDelay"></param>
    /// <param name="setup"></param>
    public static void SQLReadBuilder(string connString, DbContextOptionsBuilder options, int maxRetryCount = 3, int maxRetryDelay = 5, Action<DbContextOptionsBuilder, bool>? setup = null)
    {
        options
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSqlServer(connString, action => ServerOptionsAction(action, maxRetryCount, maxRetryDelay));
        setup?.Invoke(options, false);
    }
    /// <summary>
    /// Build write UnitOfWork
    /// </summary>
    /// <param name="migration"></param>
    /// <param name="connString"></param>
    /// <param name="options"></param>
    /// <param name="maxRetryCount"></param>
    /// <param name="maxRetryDelay"></param>
    /// <param name="setup"></param>
    public static void SQLWriteBuilder(Assembly migration, string connString, DbContextOptionsBuilder options, int maxRetryCount = 3, int maxRetryDelay = 5, Action<DbContextOptionsBuilder, bool>? setup = null)
    {
        options
            .UseSqlServer(connString, action => ServerOptionsAction(action, maxRetryCount, maxRetryDelay, migration));
        setup?.Invoke(options, true);
    }
}