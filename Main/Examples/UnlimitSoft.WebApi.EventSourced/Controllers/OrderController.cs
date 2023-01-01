using Bogus;
using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.WebApi.EventSourced.CQRS.BPL;

namespace UnlimitSoft.WebApi.EventSourced.Controllers;


[ApiController]
[Route("[controller]")]
public sealed class OrderController : ControllerBase
{
    private readonly Faker _faker;
    private readonly OrderService _service;


    public OrderController(OrderService service)
    {
        _service = service;
        _faker = new Faker();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get([FromRoute] Guid id, CancellationToken ct = default)
    {
        var order = await _service.HistoryAsync(id, ct);
        if (order is null)
            return NotFound();

        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> AddAmount([FromRoute]Guid id, [FromQuery] int amount, CancellationToken ct = default)
    {
        var order = await _service.AddAmountAsync(
            id,
            amount,
            ct
        );
        if (order is null)
            return NotFound();

        return Ok(order);
    }
    [HttpPost]
    public async Task<ActionResult> Create(CancellationToken ct = default)
    {
        var order = await _service.CreatedAsync(
            _faker.Random.String2(10),
            _faker.Random.Number(10, 100),
            ct
        );
        return Ok(order);
    }
}