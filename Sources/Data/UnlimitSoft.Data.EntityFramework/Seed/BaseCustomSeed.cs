using UnlimitSoft.Data.Seed;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.Data.EntityFramework.Seed
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
        /// <param name="unitOfWork"></param>
        /// <param name="priority"></param>
        protected BaseCustomSeed(IUnitOfWork unitOfWork, int priority = 1000)
        {
            UnitOfWork = unitOfWork;
            Priority = priority;
        }

        /// <summary>
        /// Unit of work
        /// </summary>
        protected IUnitOfWork UnitOfWork { get; }


        /// <summary>
        /// 
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public virtual async Task SeedAsync(CancellationToken ct = default)
        {
            if (typeof(TEntity).GetInterfaces().Any(t => t == typeof(IDbEnumeration)) && UnitOfWork is IDbContextWrapper dbContextWrapper)
            {
                var dbContext = dbContextWrapper.GetDbContext();
                foreach (var entry in EnumerationHelper.GetAll(typeof(TEntity)).Cast<IDbEnumeration>())
                {
                    TEntity dbEntity = await dbContext.FindAsync<TEntity>(keyValues: new [] { entry.GetId() }, cancellationToken: ct);
                    if (dbEntity != null)
                    {
                        //entity.Name = entry.Name;
                    } else
                        await dbContext.AddAsync(entry as TEntity, ct);
                }
            }
        }
    }
}
