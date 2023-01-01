using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.EventSourced.CQRS.Data;


public sealed class MyUnitOfWork : EFEventSourceDbUnitOfWork<DbContextWrite>, IMyUnitOfWork
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public MyUnitOfWork(DbContextWrite dbContext)
        : base(dbContext, null)
    {
    }
}