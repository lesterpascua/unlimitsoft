using Microsoft.EntityFrameworkCore;
using UnlimitSoft.CQRS.Data;

namespace UnlimitSoft.Data.EntityFramework
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
