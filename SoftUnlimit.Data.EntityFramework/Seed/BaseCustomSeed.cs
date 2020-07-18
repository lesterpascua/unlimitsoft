using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data.Seed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework.Seed
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class BaseCustomSeed<TEntity> : ICustomEntitySeed<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="priority"></param>
        protected BaseCustomSeed(int priority = 1000)
        {
            this.Priority = priority;
        }


        /// <summary>
        /// 
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <returns></returns>
        public virtual async Task SeedAsync(IUnitOfWork unitOfWork)
        {
            DbContext dbContext = (DbContext)unitOfWork;
            if (typeof(TEntity).GetInterfaces().Any(t => t == typeof(IDbEnumeration)))
            {
                foreach (var entry in EnumerationHelper.GetAll(typeof(TEntity)).Cast<IDbEnumeration>())
                {
                    TEntity dbEntity = await dbContext.FindAsync<TEntity>(entry.ID);
                    if (dbEntity != null)
                    {
                        //entity.Name = entry.Name;
                    } else
                        await dbContext.AddAsync(entry as TEntity);
                }
            }
        }
    }
}
