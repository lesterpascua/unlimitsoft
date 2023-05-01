using System;
using System.Collections.Generic;
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
    internal readonly List<IEvent> _events;

    /// <summary>
    /// 
    /// </summary>
    protected AggregateRoot()
    {
        _events = new List<IEvent>();
    }

    /// <inheritdoc />
    public void ClearEvents() => _events.Clear();
    /// <inheritdoc />
    public IReadOnlyCollection<IEvent> GetEvents() => _events;
}
