using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftUnlimit.WebApi.Sources.Adapter;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class ServiceController : ControllerBase
{
    private readonly ITestApiService _mockService;
    private readonly ILogger<ServiceController> _logger;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryDispatcher"></param>
    /// <param name="gen"></param>
    public ServiceController(ITestApiService mockService, ILogger<ServiceController> logger)
    {
        _mockService = mockService;
        _logger = logger;
    }

    [HttpGet("200")]
    public async Task<ActionResult<TestApiResponse>> Get200()
    {
        var response = await _mockService.Request200(default);
        var a = response;
        response.Description = "asd";
        ReferenceEquals(a, response);

        _logger.LogInformation("Response: {Response}", response);
        return Ok(response);
    }
    [HttpGet("202")]
    public async Task<ActionResult<TestApiResponse>> Get202()
    {
        var response = await _mockService.Request202(default);
        _logger.LogInformation("Response: {@Response}", response);
        return Ok(response);
    }
}
