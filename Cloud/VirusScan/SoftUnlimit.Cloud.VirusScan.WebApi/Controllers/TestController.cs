using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SoftUnlimit.Cloud.VirusScan.WebApi.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("hola");
        }
    }
}
