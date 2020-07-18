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
    public abstract class EFCQRSDbContext : EFDbContext, ICQRSUnitOfWork
    {
        #region Ctor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="eventMediator"></param>
        /// <param name="eventSourcedMediator"></param>
        protected EFCQRSDbContext(DbContextOptions options, IMediatorDispatchEvent eventMediator, IMediatorDispatchEventSourced eventSourcedMediator)
            : base(options)
        {
            this.EventMediator = eventMediator;
            this.EventSourcedMediator = eventSourcedMediator;
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
        public Task TransactionCommitAsync() => this.Database.CurrentTransaction.CommitAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task TransactionRollbackAsync() => this.Database.CurrentTransaction.RollbackAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<IAsyncDisposable> TransactionCreateAsync() => this.Database.BeginTransactionAsync().ContinueWith(c => (IAsyncDisposable)c.Result);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges() => this.SaveChangesAsync().Result;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            IDbContextTransaction transaction = this.Database.CurrentTransaction;
            if (transaction == null)
            {
                return await this.Database
                    .CreateExecutionStrategy()
                    .ExecuteAsync(() => InnerTransactionSaveChangesAsync(transaction));
            } else
                return await InnerTransactionSaveChangesAsync(transaction);
        }
        private async Task<int> InnerTransactionSaveChangesAsync(IDbContextTransaction transaction)
        {
            int changes;
            bool isTransactionOwner = false;
            if (transaction == null)
            {
                isTransactionOwner = true;
                transaction = await this.Database.BeginTransactionAsync();
            }

            try
            {
                var changedEntities = this.ChangeTracker.Entries()
                    .Where(p => p.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged)
                    .Select(s => s.Entity)
                    .ToArray();
                changes = await base.SaveChangesAsync();
                await this.PublishEvents(changedEntities);

                if (isTransactionOwner)
                    transaction.Commit();
            } finally
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
        private async Task<int> PublishEvents(IEnumerable<object> changedEntities)
        {
            Task[] tasks = new Task[2] { Task.CompletedTask, Task.CompletedTask };

            var eventMediator = this.EventMediator;
            if (eventMediator != null)
            {
                List<IEvent> events = new List<IEvent>();
                foreach (var entity in changedEntities.OfType<IAggregateRoot>())
                {
                    events.AddRange(entity.GetEvents());
                    entity.ClearEvents();
                }
                if (events.Any())
                    tasks[0] = eventMediator.DispatchEventsAsync(events);
            }

            var eventSourcedMediator = this.EventSourcedMediator;
            if (eventSourcedMediator != null)
            {
                List<IVersionedEvent> events = new List<IVersionedEvent>();
                foreach (var entity in changedEntities.OfType<IEventSourced>())
                {
                    events.AddRange(entity.GetVersionedEvents());
                    entity.ClearVersionedEvents();
                }
                if (events.Any())
                    tasks[1] = eventSourcedMediator.DispatchEventsAsync(events);
            }

            await Task.WhenAll(tasks);
            return await base.SaveChangesAsync();
        }
    }
}
