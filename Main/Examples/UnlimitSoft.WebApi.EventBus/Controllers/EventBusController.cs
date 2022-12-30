using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS.Event;
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

    [HttpPost]
    public async Task<ActionResult> GenerateEvent(CancellationToken ct = default)
    {
        var @event = new CreateEvent(Guid.NewGuid(), Guid.NewGuid());
        await _eventBus.PublishAsync(@event, ct: ct);

        _logger.LogInformation("Call");
        return Ok();
    }
}