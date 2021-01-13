using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
