using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data;


public interface IMyRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
}
