using System;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Resolver type and event associate to some string. 
/// </summary>
/// <remarks>
/// Necesary to force compiler to load assembly contain definition for this event.
/// </remarks>
public interface IEventNameResolver
{
    /// <summary>
    /// Get event asociate to supplied names.
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns>Type register with the name, null if no type asociate.</returns>
    Type? Resolver(string eventName);
}
