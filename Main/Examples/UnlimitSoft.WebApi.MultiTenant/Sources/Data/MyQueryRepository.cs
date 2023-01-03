using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class MyQueryRepository<TEntity> : EFQueryRepository<TEntity>, IMyQueryRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public MyQueryRepository(DbContextWrite dbContext)
        : base(dbContext)
    {
    }
}
