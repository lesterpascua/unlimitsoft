using Microsoft.EntityFrameworkCore;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data;
using UnlimitSoft.Data.EntityFramework;

namespace OIU.Libs.Data;


/// <summary>
/// Create a unit of work for process non requiere publish events
/// </summary>
public sealed class OIUUnitOfWork : EFDbUnitOfWork<DbContext>, IUnitOfWork
{
    /// <inheritdoc />
    public OIUUnitOfWork(IDbContextWrite dbContext)
        : base((DbContext)dbContext)
    {
    }
}
/// <summary>
/// Create unit of work for process require publish events
/// </summary>
public sealed class OIUCQRSUnitOfWork : EFCQRSDbUnitOfWork<DbContext>, IUnitOfWork
{
    /// <inheritdoc />
    public OIUCQRSUnitOfWork(IDbContextWrite dbContext, IMediatorDispatchEvent mediatorDispatch)
        : base((DbContext)dbContext, mediatorDispatch)
    {
    }
}