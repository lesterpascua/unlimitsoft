using SoftUnlimit.CQRS.Message;
using SoftUnlimit.Json;
using SoftUnlimit.Event;
using System.Net;

namespace SoftUnlimit.CQRS.Event;

/// <summary>
/// Helper methods for events.
/// </summary>
public static class IEventExtensions
{
    private static readonly IEventResponse _ok = new EventResponse<object>(HttpStatusCode.OK, null);

    /// <summary>
    /// Get body asociate to the event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="event"></param>
    /// <returns></returns>
    public static T GetBody<T>(this IEvent @event) => JsonUtility.Cast<T>(@event.GetBody());

    /// <summary>
    /// Generate a success response using event data.
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static IEventResponse OkResponse(this IEvent @this) => new EventResponse<object>(HttpStatusCode.OK, null, traceId: @this.CorrelationId);
    /// <summary>
    /// Generate a success response using event data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static IEventResponse OkResponse<T>(this IEvent @this, T body) => new EventResponse<T>(HttpStatusCode.OK, body, traceId: @this.CorrelationId);
    /// <summary>
    /// Generate a error response using event data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="this"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static IEventResponse ErrorResponse<T>(this IEvent @this, T body) => new EventResponse<T>(HttpStatusCode.OK, body, traceId: @this.CorrelationId);

    /// <summary>
    /// Generate a success response using event data.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IEventResponse QuickOkResponse(this IEvent _) => _ok;
}
