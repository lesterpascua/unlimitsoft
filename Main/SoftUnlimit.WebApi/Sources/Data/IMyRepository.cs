using SoftUnlimit.Data;

namespace SoftUnlimit.WebApi.Sources.Data
{
    public interface IMyRepository<TEntity> : IMyQueryRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
    }
}
