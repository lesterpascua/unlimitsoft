using Microsoft.EntityFrameworkCore;
using SoftUnlimit.Data;
using System.Collections;
using System.Threading.Tasks;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    public interface ICloudQueryRepository : IDbConnectionFactory
    {
        Task<IEnumerable> GetAll();
    }
    public interface ICloudQueryRepository<TEntity> : IQueryRepository<TEntity>, ICloudQueryRepository
        where TEntity : class
    {
        Task<IEnumerable> ICloudQueryRepository.GetAll() => FindAll().ToArrayAsync().ContinueWith(c => (IEnumerable)c.Result);
    }
}
