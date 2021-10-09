using Microsoft.Data.SqlClient;
using SoftUnlimit.Data.EntityFramework;

namespace SoftUnlimit.WebApi.Sources.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MyQueryRepository<TEntity> : EFQueryRepository<TEntity>, IMyQueryRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public MyQueryRepository(DbContextRead dbContext)
            : base(dbContext)
        {
        }

        protected override int GetTimeOutFromConnectionString(string connString) => new SqlConnectionStringBuilder(connString).ConnectTimeout;
    }
}
