using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SoftUnlimit.CQRS.Data;
using SoftUnlimit.CQRS.Event;
using SoftUnlimit.CQRS.EventSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        #region Ctor

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

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public IMediatorDispatchEvent EventMediator { get; }
        /// <summary>
        /// 
        /// </summary>
        public IMediatorDispatchEventSourced EventSourcedMediator { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task TransactionCommitAsync() => await DbContext.Database.CurrentTransaction.CommitAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task TransactionRollbackAsync() => await DbContext.Database.CurrentTransaction.RollbackAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IAsyncDisposable> TransactionCreateAsync() => await DbContext.Database.BeginTransactionAsync();

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
                    .ExecuteAsync(() => InnerTransactionSaveChangesAsync(transaction, allowTransaction));
            }
            else
                return await InnerTransactionSaveChangesAsync(transaction, allowTransaction);
        }

        /// <summary>
        /// Inner transaction save
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="allowTransaction"></param>
        /// <returns></returns>
        private async Task<int> InnerTransactionSaveChangesAsync(IDbContextTransaction transaction, bool allowTransaction)
        {
            int changes;
            bool isTransactionOwner = false;
            if (transaction == null && allowTransaction)
            {
                isTransactionOwner = true;
                transaction = await DbContext.Database.BeginTransactionAsync();
            }

            try
            {
                var changedEntities = DbContext.ChangeTracker.Entries()
                    .Where(p => p.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged)
                    .Select(s => s.Entity)
                    .ToArray();
                changes = await DbContext.SaveChangesAsync();
                var (count, events, versionedEvents) = await PublishEvents(changedEntities);

                if (isTransactionOwner)
                    transaction.Commit();

                if (events?.Any() == true && EventMediator != null)
                    await EventMediator.EventsDispatchedAsync(events);
                if (versionedEvents?.Any() == true && EventSourcedMediator != null)
                    await EventSourcedMediator.EventsDispatchedAsync(versionedEvents);
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
        private async Task<(int, IEnumerable<IEvent>, IEnumerable<IVersionedEvent>)> PublishEvents(IEnumerable<object> changedEntities)
        {
            List<IEvent> events = new List<IEvent>();
            List<IVersionedEvent> versionedEvents = new List<IVersionedEvent>();
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
                    tasks[0] = eventMediator.DispatchEventsAsync(events);
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
                    tasks[1] = eventSourcedMediator.DispatchEventsAsync(versionedEvents, false);
            }

            await Task.WhenAll(tasks);
            int saved = await DbContext.SaveChangesAsync();

            return (saved, events, versionedEvents);
        }
    }
}
