using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SoftUnlimit.Cloud.Partner.WebApi.Controllers
{
    /// <summary>
    /// Retrieve help information of this api
    /// </summary>
    [ApiController]
    [AllowAnonymous]
    [Route("[controller]")]
    public class HelpController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("hola");
        }
    }
}
