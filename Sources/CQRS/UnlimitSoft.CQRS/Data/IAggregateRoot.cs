using System;
using System.Collections.Generic;
using System.Threading;
using UnlimitSoft.Data;
using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Data;


/// <summary>
/// Represents an identifiable entity root in the CQRS system.
/// </summary>
public interface IAggregateRoot : IEntity
{
    /// <summary>
    /// Remove all pending event asociate to the entity.
    /// </summary>
    void ClearEvents();
    /// <summary>
    /// Gets the collection of new events since the entity was loaded, as a consequence of command handling.
    /// </summary>
    /// <returns></returns>
    IReadOnlyCollection<IEvent> GetEvents();
}
/// <summary>
/// Implement and aggregate Root
/// </summary>
public abstract class AggregateRoot : Entity<Guid>, IAggregateRoot
{
    private List<IEvent>? _events = null;

    /// <summary>
    /// 
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Get event collection
    /// </summary>
    internal List<IEvent> Events
    {
        get
        {
            if (_events is not null)
                return _events;
            Interlocked.CompareExchange(ref _events, new List<IEvent>(), null);
            return _events;
        }
    }
    /// <inheritdoc />
    public void ClearEvents() => Events.Clear();
    /// <inheritdoc />
    public IReadOnlyCollection<IEvent> GetEvents() => Events;
}
