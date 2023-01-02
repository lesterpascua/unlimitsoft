namespace UnlimitSoft.CQRS.Event;


/// <summary>
/// Messaje envelop with messaje metadata.
/// </summary>
/// <param name="Msg">Object in the envelop</param>
/// <param name="MsgType">String representation of the type of the message this will be use with <see cref="IEventNameResolver"/> to get the .net type asociate with the string type representation.</param>
public sealed record class MessageEnvelop(object Msg, string? MsgType);