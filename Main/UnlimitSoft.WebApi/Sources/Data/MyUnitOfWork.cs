using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.Sources.Data;


public class MyUnitOfWork : EFCQRSDbUnitOfWork<DbContextWrite>, IMyUnitOfWork
{
    #region Ctor
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="eventSourcedMediator"></param>
    public MyUnitOfWork(DbContextWrite dbContext, IMediatorDispatchEvent eventSourcedMediator)
        : base(dbContext, eventSourcedMediator)
    {
    }

    public override IDbConnection CreateNewDbConnection() => new SqlConnection(this.DbContext.Database.GetConnectionString());
    protected override int GetTimeOutFromConnectionString(string? connString) => connString is not null ? new SqlConnectionStringBuilder(connString).ConnectTimeout : 30;
    #endregion
}
