using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Event;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.Data.EntityFramework.Utility;

namespace UnlimitSoft.Data.EntityFramework;


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
    /// <param name="eventSourcedMediator"></param>
    protected EFCQRSDbUnitOfWork(
        TDbContext dbContext, 
        IMediatorDispatchEvent? eventSourcedMediator = null
    )
        : base(dbContext)
    {
        EventMediator = eventSourcedMediator;
    }


    /// <summary>
    /// 
    /// </summary>
    public IMediatorDispatchEvent? EventMediator { get; }

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
        if (EventMediator is null)
            return await DbContext.SaveChangesAsync(cancellationToken);

        var transaction = DbContext.Database.CurrentTransaction;
        bool allowTransaction = DbContext.Database.IsInMemory() == false;
        if (allowTransaction && transaction is null)
        {
            return await DbContext.Database
                .CreateExecutionStrategy()
                .ExecuteAsync(() => InnerTransactionSaveChangesAsync(transaction, allowTransaction, cancellationToken));
        }

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
            var changedEntities = DbContext.ChangeTracker.Entries()
                .Where(p => p.State != EntityState.Unchanged)
                .Select(s => s.Entity)
                .ToArray();
            changes = await DbContext.SaveChangesAsync(ct);
            var (count, versionedEvents) = await PublishEvents(changedEntities, ct);

            if (isTransactionOwner)
                await transaction!.CommitAsync(ct);

            if (versionedEvents?.Any() == true && EventMediator is not null)
                await EventMediator.EventsDispatchedAsync(versionedEvents, ct);
        }
        finally
        {
            if (isTransactionOwner)
                await transaction!.DisposeAsync();
        }
        return changes;
    }
    private Task PublishEventSourced(object[] changedEntities, List<IEvent> versionedEvents, CancellationToken ct)
    {
        // Publish event sourced
        var eventSourcedMediator = EventMediator;
        if (eventSourcedMediator is null)
            return Task.CompletedTask;

        foreach (var entity in changedEntities.OfType<IEventSourced>())
        {
            versionedEvents.AddRange(entity.GetEvents());
            entity.ClearEvents();
        }
        if (versionedEvents.Any())
            return eventSourcedMediator.DispatchEventsAsync(versionedEvents, false, ct);
        return Task.CompletedTask;
    }
    private async Task<(int, IEnumerable<IEvent>)> PublishEvents(object[] changedEntities, CancellationToken ct)
    {
        var versionedEvents = new List<IEvent>();

        await PublishEventSourced(changedEntities, versionedEvents, ct);
        int saved = await DbContext.SaveChangesAsync(ct);

        return (saved, versionedEvents);
    }
    #endregion
}
