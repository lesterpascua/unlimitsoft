using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.WebApi.Logger.Logger;

namespace UnlimitSoft.WebApi.Logger.Controllers;


[ApiController]
[Route("[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.TestLogger(new { a = 1 });

        _logger.LogInformation("Enter in Get");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = SysClock.GetUtcNow().AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [HttpGet(Name = "SysCall")]
    public string SysCall()
    {
        _logger.LogInformation("Took correlation from header");

        return "Took correlation from header";
    }
}