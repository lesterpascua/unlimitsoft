using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.EventSourcing.Json;
using SoftUnlimit.Data;
using SoftUnlimit.Data.EntityFramework;

namespace SoftUnlimit.WebApi.Sources.CQRS.Event
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMyVersionedEventRepository : IRepository<JsonVersionedEventPayload>
    {
    }
    /// <summary>
    /// Implementation using database storage.
    /// </summary>
    public class MyVersionedEventRepository<TDbContext> : EFRepository<JsonVersionedEventPayload>, IMyVersionedEventRepository
        where TDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        public MyVersionedEventRepository(TDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
