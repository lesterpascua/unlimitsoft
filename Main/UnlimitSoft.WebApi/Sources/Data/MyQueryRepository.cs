using Microsoft.Data.SqlClient;
using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.Sources.Data;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public sealed class MyQueryRepository<TEntity> : EFQueryRepository<DbContextRead, TEntity>, IMyQueryRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public MyQueryRepository(DbContextRead dbContext)
        : base(dbContext)
    {
    }

    protected override int GetTimeOutFromConnectionString(string? connString) => new SqlConnectionStringBuilder(connString).ConnectTimeout;
}
