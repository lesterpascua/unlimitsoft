using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data;


public sealed class MyUnitOfWork : EFDbUnitOfWork<DbContextWrite>, IMyUnitOfWork
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public MyUnitOfWork(DbContextWrite dbContext)
        : base(dbContext)
    {
    }
}