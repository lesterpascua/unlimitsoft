using Microsoft.EntityFrameworkCore;
using UnlimitSoft.Data;
using UnlimitSoft.Data.EntityFramework;

namespace OIU.Libs.Data;


/// <summary>
/// Implement a readonly repository
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public sealed class OIUQueryRepository<TEntity> : EFQueryRepository<DbContext, TEntity>, IQueryRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public OIUQueryRepository(IDbContextRead dbContext)
        : base((DbContext)dbContext)
    {
    }
}