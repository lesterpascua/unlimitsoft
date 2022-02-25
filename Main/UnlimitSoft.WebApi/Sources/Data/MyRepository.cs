using SoftUnlimit.Data.EntityFramework;

namespace SoftUnlimit.WebApi.Sources.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class MyRepository<TEntity> : EFRepository<TEntity>, IMyRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public MyRepository(DbContextWrite dbContext)
            : base(dbContext)
        {
        }
    }
}
