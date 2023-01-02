using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UnlimitSoft.MultiTenant.AspNet;
using UnlimitSoft.WebApi.MultiTenant.Sources.Configuration;

namespace UnlimitSoft.WebApi.EventBus.Controllers;


[ApiController]
[Route("[controller]")]
public sealed class CustomerController : ControllerBase
{
    private readonly ServiceOptions _options;
    private readonly ILogger<CustomerController> _logger;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    public CustomerController(IOptions<ServiceOptions> options, ILogger<CustomerController> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tenant"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost]
    public ActionResult Post([FromQuery] string tenant, CancellationToken ct = default)
    {
        return Ok(HttpContext.GetTenant());
    }
}