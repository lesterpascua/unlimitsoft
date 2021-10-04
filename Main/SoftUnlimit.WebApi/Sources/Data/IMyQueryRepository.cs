using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data;
using System.Collections;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.Data
{
    public interface IMyQueryRepository
    {
        Task<IEnumerable> GetAll();
    }
    public interface IMyQueryRepository<TEntity> : IQueryRepository<TEntity>, IMyQueryRepository
        where TEntity : class
    {
        Task<IEnumerable> IMyQueryRepository.GetAll() => FindAll().ToArrayAsync().ContinueWith(c => (IEnumerable)c.Result);
    }
}
