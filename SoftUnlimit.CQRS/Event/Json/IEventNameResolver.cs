using System;

namespace SoftUnlimit.CQRS.Event.Json
{
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
        /// <returns></returns>
        Type Resolver(string eventName);
    }
}
