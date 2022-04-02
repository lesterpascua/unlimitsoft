using SoftUnlimit.CQRS.Event.Json;

namespace SoftUnlimit.CQRS.Event;

/// <summary>
/// Messaje envelop with messaje metadata.
/// </summary>
public class MessageEnvelop
{
    /// <summary>
    /// How object is coding as a json of as a object
    /// </summary>
    public MessageType Type { get; set; }

    /// <summary>
    /// Object in the envelop if <see cref="MessageType.Json"/> will be an string codign as json object, see <see cref="MessageType"/> for more info.
    /// </summary>
    public object Msg { get; set; }
    /// <summary>
    /// String representation of the type of the message this will be use with <see cref="IEventNameResolver"/> to get the .net type asociate with the string type representation.
    /// </summary>
    public string MsgType { get; set; }
}
