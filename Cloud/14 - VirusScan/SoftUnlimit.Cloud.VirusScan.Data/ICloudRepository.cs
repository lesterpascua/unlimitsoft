using SoftUnlimit.Data;

namespace SoftUnlimit.Cloud.VirusScan.Data
{
    public interface ICloudRepository<TEntity> : ICloudQueryRepository<TEntity>, IRepository<TEntity> where TEntity : class
    {
    }
}
