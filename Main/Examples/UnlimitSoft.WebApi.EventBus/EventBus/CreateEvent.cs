﻿using System.Text.Json.Serialization;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.EventBus.EventBus;


/// <summary>
/// 
/// </summary>
public class CreateEvent : Event<string?>
{
    [JsonConstructor]
    public CreateEvent() { }

    public CreateEvent(Guid id, Guid sourceId, ushort serviceId = 0, string? workerId = null, string? correlationId = null, bool isDomain = false, string? body = null) : 
        base(id, sourceId, 0, serviceId, workerId, correlationId, isDomain, body)
    {
    }
}
