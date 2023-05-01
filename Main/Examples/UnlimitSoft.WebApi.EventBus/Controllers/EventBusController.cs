using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS.Data.Dto;
using UnlimitSoft.CQRS.Event;
using UnlimitSoft.Json;
using UnlimitSoft.WebApi.EventBus.EventBus;

namespace UnlimitSoft.WebApi.EventBus.Controllers;


[ApiController]
[Route("[controller]")]
public class EventBusController : ControllerBase
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventBusController> _logger;

    public EventBusController(IEventBus eventBus, ILogger<EventBusController> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    [HttpPost("event")]
    public async Task<ActionResult> Event(CancellationToken ct = default)
    {
        var @event = new CreateEvent(Guid.NewGuid(), Guid.NewGuid(), body: "This is a body");
        await _eventBus.PublishAsync(@event, ct: ct);

        _logger.LogInformation("Publish event {@event}", @event);
        return Ok();
    }

    [HttpPost("payload")]
    public async Task<ActionResult> Payload(CancellationToken ct = default)
    {
        var @event = new CreateEvent(Guid.NewGuid(), Guid.NewGuid(), body: "This is a body");
        var payload = new EventPayload(@event, JsonUtil.Default);
        await _eventBus.PublishPayloadAsync(payload, ct: ct);

        _logger.LogInformation("Publish payload {@event}", payload);
        return Ok();
    }
}