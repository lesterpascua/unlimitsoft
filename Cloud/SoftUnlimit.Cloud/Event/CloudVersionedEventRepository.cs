using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;

namespace SoftUnlimit.Cloud.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICloudVersionedEventRepository : IRepository<JsonVersionedEventPayload>
    {
    }
    /// <summary>
    /// Implementation using database storage.
    /// </summary>
    public class CloudVersionedEventRepository<TDbContext> : EFRepository<JsonVersionedEventPayload>, ICloudVersionedEventRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public CloudVersionedEventRepository(TDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
