using UnlimitSoft.CQRS.Event.Json;

namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Messaje envelop with messaje metadata.
/// </summary>
/// <param name="Type">How object is coding as a json of as a object</param>
/// <param name="Msg">Object in the envelop if <see cref="MessageType.Json"/> will be an string codign as json object, see <see cref="MessageType"/> for more info.</param>
/// <param name="MsgType">String representation of the type of the message this will be use with <see cref="IEventNameResolver"/> to get the .net type asociate with the string type representation.</param>
public record class MessageEnvelop(MessageType Type, object Msg, string? MsgType);