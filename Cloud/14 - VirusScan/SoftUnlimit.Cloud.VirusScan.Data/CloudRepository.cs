using SoftUnlimit.Data.EntityFramework;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CloudRepository<TEntity> : EFRepository<TEntity>, ICloudRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public CloudRepository(DbContextWrite dbContext)
            : base(dbContext)
        {
        }
    }
}
