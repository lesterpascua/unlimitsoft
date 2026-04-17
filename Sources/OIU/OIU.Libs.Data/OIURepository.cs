using Microsoft.EntityFrameworkCore;
using UnlimitSoft.Data;
using UnlimitSoft.Data.EntityFramework;

namespace OIU.Libs.Data;


/// <inheritdoc />
public sealed class OIURepository<TEntity> : EFRepository<DbContext, TEntity>, IRepository<TEntity>
    where TEntity : class
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    public OIURepository(IDbContextWrite dbContext)
        : base((DbContext)dbContext)
    {
    }
}