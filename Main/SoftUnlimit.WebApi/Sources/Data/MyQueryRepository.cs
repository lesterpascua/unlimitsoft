using SoftUnlimit.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
