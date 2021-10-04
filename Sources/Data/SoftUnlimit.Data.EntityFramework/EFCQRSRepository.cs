using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Data;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EFCQRSRepository<TEntity> : EFRepository<TEntity>, ICQRSRepository<TEntity>
        where TEntity : class, IAggregateRoot
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public EFCQRSRepository(DbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
