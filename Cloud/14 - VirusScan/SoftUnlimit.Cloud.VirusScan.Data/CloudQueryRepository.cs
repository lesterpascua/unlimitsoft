using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.EntityFramework;
using System.Data;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CloudQueryRepository<TEntity> : EFQueryRepository<TEntity>, ICloudQueryRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public CloudQueryRepository(DbContextRead dbContext)
            : base(dbContext)
        {
        }

        public override IDbConnection CreateNewDbConnection() => new SqlConnection(DbContext.Database.GetConnectionString());
        protected override int GetTimeOutFromConnectionString(string connString) => new SqlConnectionStringBuilder(connString).ConnectTimeout;
    }
}
