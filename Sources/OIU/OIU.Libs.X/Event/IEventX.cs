using System.Runtime.CompilerServices;
using UnlimitSoft.Message;

namespace OIU.Libs.Event;


/// <summary>
/// Event with more values
/// </summary>
public interface IEventX : IEvent
{
    /// <summary>
    /// User owner of the event. This field will be filled to help better event trace.
    /// </summary>
    Guid IdentityId { get; set; }
    /// <summary>
    /// Micro service where the event is originate in first place. Some event like scan file for virus move over several service this allow 
    /// us to identify the original service where the event was created
    /// </summary>
    ushort SourceMicroServiceId { get; set; }
}
/// <summary>
/// Base event class for OneJN System
/// </summary>
public abstract class EventX<TBody> : Event<TBody>, IEventX, IProps
{
    /// <summary>
    /// Empty constructor require for deserialization.
    /// </summary>
    protected EventX() { }
    /// <summary>
    /// Constructor to initialize the event
    /// </summary>
    /// <param name="id">Unique event identifier</param>
    /// <param name="sourceId">Identifier of the entity source of the event</param>
    /// <param name="version">Version of the event</param>
    /// <param name="serviceId">Identifier of the service owner of the event</param>
    /// <param name="workerId">Identifier of the worker hows generate the event</param>
    /// <param name="correlationId">Correlation asociate to the event</param>
    /// <param name="body">Information asociate to the event</param>
    protected EventX(Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, TBody body)
        : base(id, sourceId, version, serviceId, workerId, correlationId, false, body)
    {
    }

    /// <inheritdoc />
    public Guid IdentityId { get; set; }
    /// <inheritdoc />
    public ushort SourceMicroServiceId { get; set; }

    /// <inheritdoc />
    string? IProps.CorrelationId => CorrelationId;

    /// <summary>
    /// Create new instance of the event.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="id"></param>
    /// <param name="sourceId"></param>
    /// <param name="version"></param>
    /// <param name="serviceId"></param>
    /// <param name="workerId"></param>
    /// <param name="correlationId"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEventX New(Type type, Guid id, Guid sourceId, long version, ushort serviceId, string? workerId, string? correlationId, object? body) => (IEventX)Activator.CreateInstance(type, id, sourceId, version, serviceId, workerId, correlationId, body)!;
}

/// <summary>
/// 
/// </summary>
public static class IEventXExtensions
{
    /// <summary>
    /// Indicate if the identity of the event is the system
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static bool IsFromSystem(this IEventX @this)
    {
        return @this.IdentityId == Constants.Security.Scope.System.IdentityId;
    }
    /// <summary>
    /// Indicate if the identity of the event is anonymous
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static bool IsFromAnonymous(this IEventX @this)
    {
        return @this.IdentityId == Constants.Security.Role.Anonymous.IdentityId;
    }
}