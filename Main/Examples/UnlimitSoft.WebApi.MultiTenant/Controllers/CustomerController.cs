using Microsoft.AspNetCore.Mvc;

namespace UnlimitSoft.WebApi.EventBus.Controllers;


[ApiController]
[Route("[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ILogger<CustomerController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> Insert(CancellationToken ct = default)
    {
        return Ok();
    }

}