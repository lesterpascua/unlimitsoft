using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.WebApi.CommandBus.Commands;

namespace UnlimitSoft.WebApi.CommandBus.Controllers;


[ApiController]
[Route("[controller]")]
public sealed class CommandController : ControllerBase
{
    private readonly ICommandBus _bus;

    public CommandController(ICommandBus bus)
    {
        _bus = bus;
    }

    [HttpPost]
    public async Task<ActionResult> CreateAsync(CancellationToken ct = default)
    {
        var response = await _bus.SendAsync(new TestCommand(), ct);
        return Ok(response);
    }
}