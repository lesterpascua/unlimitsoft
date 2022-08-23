using UnlimitSoft.Event;

namespace UnlimitSoft.WebApi.EventBus.EventBus;


/// <summary>
/// 
/// </summary>
public class CreateEvent : Event<int, string>
{
    public CreateEvent(Guid id, int sourceId, ushort serviceId = 0, string? workerId = null, string? correlationId = null, object? command = null, object? prevState = null, object? currState = null, bool isDomain = false, string? body = null) : 
        base(id, sourceId, serviceId, workerId, correlationId, command, prevState, currState, isDomain, body)
    {
    }
}
