using OIU.Libs.Event;
using UnlimitSoft.Json;
using UnlimitSoft.Message;

namespace OIU.Libs.Events;


/// <summary>
/// Dto of event representation
/// </summary>
public sealed class OIUEventPayload : EventPayload
{
    /// <summary>
    /// 
    /// </summary>
    public OIUEventPayload()
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="event"></param>
    /// <param name="serializer"></param>
    public OIUEventPayload(IEventX @event, IJsonSerializer serializer)
        : base(@event, serializer)
    {
        IdentityId = @event.IdentityId;
        SourceMicroServiceId = @event.SourceMicroServiceId;
    }

    /// <summary>
    /// User owner of the event. This field will be filled to help better event trace.
    /// </summary>
    public Guid IdentityId { get; set; }
    /// <summary>
    /// Micro service where the event is originate in first place. Some event like scan file for virus move over several service this allow 
    /// us to identify the original service where the event was created
    /// </summary>
    public ushort SourceMicroServiceId { get; set; }

    /// <summary>
    /// Populate extra field included in <see cref="OIUEventPayload"/>
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="e"></param>
    public static void PopulateJnPayload<TEvent>(OIUEventPayload payload, TEvent e)
        where TEvent : IEventX
    {
        e.IdentityId = payload.IdentityId;
        e.SourceMicroServiceId = payload.SourceMicroServiceId;
    }
    /// <summary>
    /// Load event from <see cref="OIUEventPayload"/>
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TBody"></typeparam>
    /// <param name="payload"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public static TEvent FromJnPayload<TEvent, TBody>(OIUEventPayload payload, IJsonSerializer serializer)
        where TEvent : IEventX
    {
        var e = (TEvent)FromEventPayload(typeof(TEvent), typeof(TBody), payload, serializer);
        PopulateJnPayload(payload, e);

        return e;
    }
}