using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.Sources.Data
{
    public interface IMyRepository<TEntity> : IMyQueryRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
    }
}
