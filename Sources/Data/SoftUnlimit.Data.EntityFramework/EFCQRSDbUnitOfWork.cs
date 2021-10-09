using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.Data.EntityFramework
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class EFCQRSDbUnitOfWork<TDbContext> : EFDbUnitOfWork<TDbContext>, ICQRSUnitOfWork
        where TDbContext : DbContext
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="eventMediator"></param>
        /// <param name="eventSourcedMediator"></param>
        protected EFCQRSDbUnitOfWork(TDbContext dbContext, IMediatorDispatchEvent eventMediator = null, IMediatorDispatchEventSourced eventSourcedMediator = null)
            : base(dbContext)
        {
            EventMediator = eventMediator;
            EventSourcedMediator = eventSourcedMediator;
        }


        /// <summary>
        /// 
        /// </summary>
        public IMediatorDispatchEvent EventMediator { get; }
        /// <summary>
        /// 
        /// </summary>
        public IMediatorDispatchEventSourced EventSourcedMediator { get; }


        /// <inheritdoc />
        public async Task TransactionCommitAsync(CancellationToken ct) => await DbContext.Database.CurrentTransaction.CommitAsync(ct);
        /// <inheritdoc />
        public async Task TransactionRollbackAsync(CancellationToken ct) => await DbContext.Database.CurrentTransaction.RollbackAsync(ct);
        /// <inheritdoc />
        public async Task<IAsyncDisposable> TransactionCreateAsync(IsolationLevel level, CancellationToken ct) => await DbContext.Database.BeginTransactionAsync(level, ct);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges() => SaveChangesAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            IDbContextTransaction transaction = DbContext.Database.CurrentTransaction;
            bool allowTransaction = DbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory";
            if (allowTransaction && transaction == null)
            {
                return await DbContext.Database
                    .CreateExecutionStrategy()
                    .ExecuteAsync(() => InnerTransactionSaveChangesAsync(transaction, allowTransaction, cancellationToken));
            }
            else
                return await InnerTransactionSaveChangesAsync(transaction, allowTransaction, cancellationToken);
        }


        #region Private Methods
        /// <summary>
        /// Inner transaction save
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="allowTransaction"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task<int> InnerTransactionSaveChangesAsync(IDbContextTransaction transaction, bool allowTransaction, CancellationToken ct)
        {
            int changes;
            bool isTransactionOwner = false;
            if (transaction == null && allowTransaction)
            {
                isTransactionOwner = true;
                transaction = await DbContext.Database.BeginTransactionAsync(ct);
            }

            try
            {
                var changedEntities = DbContext.ChangeTracker.Entries()
                    .Where(p => p.State != EntityState.Unchanged)
                    .Select(s => s.Entity)
                    .ToArray();
                changes = await DbContext.SaveChangesAsync(ct);
                var (count, events, versionedEvents) = await PublishEvents(changedEntities, ct);

                if (isTransactionOwner)
                    await transaction.CommitAsync(ct);

                if (events?.Any() == true && EventMediator != null)
                    await EventMediator.EventsDispatchedAsync(events, ct);
                if (versionedEvents?.Any() == true && EventSourcedMediator != null)
                    await EventSourcedMediator.EventsDispatchedAsync(versionedEvents, ct);
            }
            finally
            {
                if (isTransactionOwner)
                    await transaction.DisposeAsync();
            }
            return changes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<(int, IEnumerable<IEvent>, IEnumerable<IVersionedEvent>)> PublishEvents(IEnumerable<object> changedEntities, CancellationToken ct)
        {
            var events = new List<IEvent>();
            var versionedEvents = new List<IVersionedEvent>();
            Task[] tasks = new Task[2] { Task.CompletedTask, Task.CompletedTask };

            var eventMediator = EventMediator;
            if (eventMediator != null)
            {
                foreach (var entity in changedEntities.OfType<IAggregateRoot>())
                {
                    events.AddRange(entity.GetEvents());
                    entity.ClearEvents();
                }
                if (events.Any())
                    tasks[0] = eventMediator.DispatchEventsAsync(events, ct);
            }

            var eventSourcedMediator = EventSourcedMediator;
            if (eventSourcedMediator != null)
            {
                foreach (var entity in changedEntities.OfType<IEventSourced>())
                {
                    versionedEvents.AddRange(entity.GetVersionedEvents());
                    entity.ClearVersionedEvents();
                }
                if (versionedEvents.Any())
                    tasks[1] = eventSourcedMediator.DispatchEventsAsync(versionedEvents, false, ct);
            }

            await Task.WhenAll(tasks);
            int saved = await DbContext.SaveChangesAsync(ct);

            return (saved, events, versionedEvents);
        }
        #endregion
    }
}
