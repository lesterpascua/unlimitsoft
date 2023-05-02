using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Data.EntityFramework.Utility;
using UnlimitSoft.Message;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public abstract class EFMediatorDbUnitOfWork<TDbContext> : EFDbUnitOfWork<TDbContext>, ICQRSUnitOfWork
    where TDbContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="eventMediator"></param>
    protected EFMediatorDbUnitOfWork(TDbContext dbContext, IMediatorDispatchEvent? eventMediator = null)
        : base(dbContext)
    {
        StopCreation = false;
        EventMediator = eventMediator;
    }

    /// <summary>
    /// Indicate if the mediator will create the event or not. 
    /// </summary>
    public bool StopCreation { get; protected set; }
    /// <summary>
    /// Mediator used to manipulate the save of the events
    /// </summary>
    public IMediatorDispatchEvent? EventMediator { get; }

    /// <summary>
    /// Get all entities pending for update
    /// </summary>
    /// <returns></returns>
    protected abstract object[] GetPendingEntities();
    /// <summary>
    /// Extract events from the entities pending for change
    /// </summary>
    /// <param name="changes"></param>
    /// <param name="events"></param>
    protected abstract void CollectEventsFromEntities(object[] changes, List<IEvent> events);

    /// <summary>
    /// Save pending change in the database <see cref="DbContext.SaveChanges()"/>
    /// </summary>
    /// <returns></returns>
    public override int SaveChanges() => SaveChangesAsync().Result;
    /// <summary>
    /// Save pending change in the database <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        if (EventMediator is null)
            return await DbContext.SaveChangesAsync(ct);

        var transaction = DbContext.Database.CurrentTransaction;
        bool allowTransaction = DbContext.Database.IsInMemory() == false;
        if (allowTransaction && transaction is null)
        {
            return await DbContext.Database
                .CreateExecutionStrategy()
                .ExecuteAsync(() => InnerTransactionSaveChangesAsync(transaction, allowTransaction, ct));
        }

        return await InnerTransactionSaveChangesAsync(transaction, allowTransaction, ct);
    }

    #region Private Methods
    /// <summary>
    /// Collect event and publis throght mediator.
    /// </summary>
    /// <param name="changes"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<(int, List<IEvent>)> PublishEvents(object[] changes, CancellationToken ct)
    {
        var events = new List<IEvent>();

        // Publish event only if exist a mediator
        var mediator = EventMediator;
        if (mediator is not null)
        {
            CollectEventsFromEntities(changes, events);
            if (events.Count != 0)
                await mediator.DispatchEventsAsync(events, false, StopCreation, ct);
        }

        // Save pending change this is inside of a transaction so result will not reflected until de commit
        int saved = await DbContext.SaveChangesAsync(ct);
        return (saved, events);
    }
    /// <summary>
    /// Inner transaction save
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="allowTransaction"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<int> InnerTransactionSaveChangesAsync(IDbContextTransaction? transaction, bool allowTransaction, CancellationToken ct)
    {
        int changes;
        bool isTransactionOwner = false;
        if (transaction is null && allowTransaction)
        {
            isTransactionOwner = true;
            transaction = await DbContext.Database.BeginTransactionAsync(ct);
        }

        try
        {
            var changed = GetPendingEntities();
            changes = await DbContext.SaveChangesAsync(ct);
            var (count, versionedEvents) = await PublishEvents(changed, ct);

            if (isTransactionOwner)
                await transaction!.CommitAsync(ct);

            if (versionedEvents.Count != 0 && EventMediator is not null)
                await EventMediator.EventsDispatchedAsync(versionedEvents, ct);
        }
        finally
        {
            if (isTransactionOwner)
                await transaction!.DisposeAsync();
        }
        return changes;
    }
    #endregion
}
