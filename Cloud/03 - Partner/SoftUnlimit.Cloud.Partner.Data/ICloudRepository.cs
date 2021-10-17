using SoftUnlimit.Data;

namespace SoftUnlimit.Cloud.Partner.Data
{
    public interface ICloudRepository<TEntity> : ICloudQueryRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
    }
}
