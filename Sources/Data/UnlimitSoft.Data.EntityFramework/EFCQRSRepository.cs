using Microsoft.EntityFrameworkCore;
using UnlimitSoft.CQRS.Data;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class EFCQRSRepository<TContext, TEntity> : EFRepository<TContext, TEntity>, ICQRSRepository<TEntity> 
    where TEntity : class, IAggregateRoot
    where TContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public EFCQRSRepository(TContext dbContext)
        : base(dbContext)
    {
    }
}
