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
    /// Get name asociate to supplied event type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    string? Resolver(Type type);
    /// <summary>
    /// Get event asociate to supplied names.
    /// </summary>
    /// <param name="eventName"></param>
    /// <returns>Type register with the name, null if no type asociate.</returns>
    Type? Resolver(string eventName);
}
/// <summary>
/// 
/// </summary>
public static class IEventNameResolverExtensions
{
    /// <summary>
    /// Get event asociate to supplied names.
    /// </summary>
    /// <param name="resolver"></param>
    /// <param name="eventName"></param>
    /// <returns>Type register with the name, null if no type asociate</returns>
    /// <exception cref="InvalidOperationException">If the event name is not register in the resolver</exception>
    public static Type RequireResolver(this IEventNameResolver resolver, string eventName)
    {
        return resolver.Resolver(eventName) ?? throw new InvalidOperationException($"Event name={eventName} can't resolve");
    }
}