using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.EventSourcing;
using SoftUnlimit.Data.EntityFramework;
using System.Data;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    public class CloudUnitOfWork : EFCQRSDbUnitOfWork<DbContextWrite>, ICloudUnitOfWork
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="eventSourcedMediator"></param>
        public CloudUnitOfWork(DbContextWrite dbContext, IMediatorDispatchEventSourced eventSourcedMediator)
            : base(dbContext, null, eventSourcedMediator)
        {
        }

        public override IDbConnection CreateNewDbConnection() => new SqlConnection(DbContext.Database.GetConnectionString());
        protected override int GetTimeOutFromConnectionString(string connString) => new SqlConnectionStringBuilder(connString).ConnectTimeout;
    }
}
