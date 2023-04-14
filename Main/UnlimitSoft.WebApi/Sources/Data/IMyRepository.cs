using UnlimitSoft.Data;
using UnlimitSoft.Data.EntityFramework;

namespace UnlimitSoft.WebApi.Sources.Data;


public interface IMyRepository<TEntity> : IRepository<TEntity>, IEFRepository<DbContextWrite, TEntity> where TEntity : class
{
}
