using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.EntityFramework;
using System.Data;

namespace SoftUnlimit.Cloud.Partner.Data
{
    public class CloudUnitOfWork : EFDbUnitOfWork<DbContextWrite>, ICloudUnitOfWork
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public CloudUnitOfWork(DbContextWrite dbContext)
            : base(dbContext)
        {
        }

        public override IDbConnection CreateNewDbConnection() => new SqlConnection(DbContext.Database.GetConnectionString());
        protected override int GetTimeOutFromConnectionString(string connString) => new SqlConnectionStringBuilder(connString).ConnectTimeout;
    }
}
