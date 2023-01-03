using Microsoft.EntityFrameworkCore;
using System.Collections;
using UnlimitSoft.Data;

namespace UnlimitSoft.WebApi.MultiTenant.Sources.Data;


public interface IMyQueryRepository
{
    Task<IEnumerable> GetAll();
}
public interface IMyQueryRepository<TEntity> : IQueryRepository<TEntity>, IMyQueryRepository
    where TEntity : class
{
    Task<IEnumerable> IMyQueryRepository.GetAll() => FindAll().ToArrayAsync().ContinueWith(c => (IEnumerable)c.Result);
}
