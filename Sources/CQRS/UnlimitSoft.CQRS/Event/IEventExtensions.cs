using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace UnlimitSoft.Event;


/// <summary>
/// 
/// </summary>
public static class IEventExtensions
{
    /// <summary>
    /// Get body asociate to the event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="event"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public static T? GetBody<T>(this IEvent @event, IJsonSerializer serializer) => serializer.Cast<T>(@event.GetBody());
}