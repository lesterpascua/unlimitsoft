using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.Sources.Data;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class MyRepository<TEntity> : EFRepository<DbContextWrite, TEntity>, IMyRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public MyRepository(DbContextWrite dbContext)
        : base(dbContext)
    {
    }
}
