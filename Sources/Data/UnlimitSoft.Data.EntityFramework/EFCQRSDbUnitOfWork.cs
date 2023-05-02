using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnlimitSoft.CQRS.Data;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Message;

namespace UnlimitSoft.Data.EntityFramework;


/// <summary>
/// 
/// </summary>
public abstract class EFCQRSDbUnitOfWork<TDbContext> : EFMediatorDbUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="eventMediator"></param>
    protected EFCQRSDbUnitOfWork(TDbContext dbContext, IMediatorDispatchEvent? eventMediator = null)
        : base(dbContext, eventMediator)
    {
        StopCreation = false;
    }

    /// <summary>
    /// Get all entities pending for update
    /// </summary>
    /// <returns></returns>
    protected override object[] GetPendingEntities()
    {
        var changes = DbContext.ChangeTracker
            .Entries()
            .Where(p => p.State != EntityState.Unchanged)
            .Select(s => s.Entity)
            .ToArray();
        return changes;
    }
    /// <summary>
    /// Extract events from the entities pending for change
    /// </summary>
    /// <param name="changes"></param>
    /// <param name="events"></param>
    protected override void CollectEventsFromEntities(object[] changes, List<IEvent> events)
    {
        for (var i = 0; i < changes.Length; i++)
        {
            var entity = changes[i];
            if (entity is not IEventSourced es)
                continue;

            events.AddRange(es.GetEvents());
            es.ClearEvents();
        }
    }
}
