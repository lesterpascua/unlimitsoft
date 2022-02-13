using Microsoft.EntityFrameworkCore;
using SoftUnlimit.CQRS.Event;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public abstract class EFEventSourceDbUnitOfWork<TDbContext> : EFDbUnitOfWork<TDbContext>
        where TDbContext : DbContext
    {
        private readonly IEventPublishWorker _publishWorker;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="publishWorker"></param>
        protected EFEventSourceDbUnitOfWork(TDbContext dbContext, IEventPublishWorker publishWorker)
            : base(dbContext)
        {
            _publishWorker = publishWorker;
        }

        /// <inheritdoc />
        public override int SaveChanges() => SaveChangesAsync().Result;
        /// <inheritdoc />
        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var changed = DbContext.ChangeTracker
                .Entries()
                .Where(p => p.State != EntityState.Unchanged && p.Entity is EventPayload)
                .Select(s =>
                {
                    var e = (EventPayload)s.Entity;
                    return new PublishEventInfo(e.Id, e.Created, e.Scheduled);
                })
                .ToArray();

            var count = await DbContext.SaveChangesAsync(ct);
            if (_publishWorker is not null)
                await _publishWorker.PublishAsync(changed, ct);
            return count;
        }
    }
}