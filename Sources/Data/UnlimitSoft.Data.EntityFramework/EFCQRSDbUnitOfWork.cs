﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.CQRS.EventSourcing;
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
    /// <param name="eventMediator"></param>
    /// <param name="eventSourcedMediator"></param>
    protected EFCQRSDbUnitOfWork(
        TDbContext dbContext, 
        IMediatorDispatchEvent? eventMediator = null, 
        IMediatorDispatchEventSourced? eventSourcedMediator = null
    )
        : base(dbContext)
    {
        EventMediator = eventMediator;
        EventSourcedMediator = eventSourcedMediator;
    }


    /// <summary>
    /// 
    /// </summary>
    public IMediatorDispatchEvent? EventMediator { get; }
    /// <summary>
    /// 
    /// </summary>
    public IMediatorDispatchEventSourced? EventSourcedMediator { get; }

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
        if (EventMediator is null && EventSourcedMediator is null)
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
            var (count, events, versionedEvents) = await PublishEvents(changedEntities, ct);

            if (isTransactionOwner)
                await transaction!.CommitAsync(ct);

            if (events?.Any() == true && EventMediator != null)
                await EventMediator.EventsDispatchedAsync(events, ct);
            if (versionedEvents?.Any() == true && EventSourcedMediator != null)
                await EventSourcedMediator.EventsDispatchedAsync(versionedEvents, ct);
        }
        finally
        {
            if (isTransactionOwner)
                await transaction!.DisposeAsync();
        }
        return changes;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Task PublishEvents(object[] changedEntities, List<IEvent> events, CancellationToken ct)
    {
        // Publish regular events
        var eventMediator = EventMediator;
        if (eventMediator is null)
            return Task.CompletedTask;

        foreach (var entity in changedEntities.OfType<IAggregateRoot>())
        {
            events.AddRange(entity.GetEvents());
            entity.ClearEvents();
        }
        if (events.Any())
            return eventMediator.DispatchEventsAsync(events, ct);
        return Task.CompletedTask;
    }
    private Task PublishEventSourced(object[] changedEntities, List<IVersionedEvent> versionedEvents, CancellationToken ct)
    {
        // Publish event sourced
        var eventSourcedMediator = EventSourcedMediator;
        if (eventSourcedMediator is null)
            return Task.CompletedTask;

        foreach (var entity in changedEntities.OfType<IEventSourced>())
        {
            versionedEvents.AddRange(entity.GetVersionedEvents());
            entity.ClearVersionedEvents();
        }
        if (versionedEvents.Any())
            return eventSourcedMediator.DispatchEventsAsync(versionedEvents, false, ct);
        return Task.CompletedTask;
    }
    private async Task<(int, IEnumerable<IEvent>, IEnumerable<IVersionedEvent>)> PublishEvents(object[] changedEntities, CancellationToken ct)
    {
        var tasks = new Task[2];
        var events = new List<IEvent>();
        var versionedEvents = new List<IVersionedEvent>();

        tasks[0] = PublishEvents(changedEntities, events, ct);
        tasks[1] = PublishEventSourced(changedEntities, versionedEvents, ct);

        Task.WaitAll(tasks, ct);
        int saved = await DbContext.SaveChangesAsync(ct);

        return (saved, events, versionedEvents);
    }
    #endregion
}
