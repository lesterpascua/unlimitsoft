using Microsoft.EntityFrameworkCore;
using UnlimitSoft.Data;
using System.Collections;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.Data
{
    public interface IMyQueryRepository : IDbConnectionFactory
    {
        Task<IEnumerable> GetAll();
    }
    public interface IMyQueryRepository<TEntity> : IQueryRepository<TEntity>, IMyQueryRepository
        where TEntity : class
    {
        Task<IEnumerable> IMyQueryRepository.GetAll() => FindAll().ToArrayAsync().ContinueWith(c => (IEnumerable)c.Result);
    }
}
