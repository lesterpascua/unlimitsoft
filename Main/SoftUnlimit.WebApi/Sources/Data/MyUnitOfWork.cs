using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data.EntityFramework;
using System.Data;

namespace SoftUnlimit.WebApi.Sources.Data
{
    public class MyUnitOfWork : EFCQRSDbUnitOfWork<DbContextWrite>, IMyUnitOfWork
    {
        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="eventSourcedMediator"></param>
        public MyUnitOfWork(DbContextWrite dbContext, IMediatorDispatchEventSourced eventSourcedMediator)
            : base(dbContext, null, eventSourcedMediator)
        {
        }

        public override IDbConnection CreateNewDbConnection() => new SqlConnection(this.DbContext.Database.GetConnectionString());
        protected override int GetTimeOutFromConnectionString(string connString) => new SqlConnectionStringBuilder(connString).ConnectTimeout;
        #endregion
    }
}
