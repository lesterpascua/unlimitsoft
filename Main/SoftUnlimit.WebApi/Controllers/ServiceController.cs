using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Security.Cryptography;
using SoftUnlimit.Web.Client;
using SoftUnlimit.WebApi.Sources.Adapter;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using SoftUnlimit.WebApi.Sources.Data.Model;
using SoftUnlimit.WebApi.Sources.Security.Cryptography;
using SoftUnlimit.WebApi.Sources.Web;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class ServiceController : ControllerBase
    {
        private readonly IMockService _mockService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryDispatcher"></param>
        /// <param name="gen"></param>
        public ServiceController(IMockService mockService)
        {
            _mockService = mockService;
        }

        [HttpGet("200")]
        public async Task<ActionResult<MockResponse>> Get200()
        {
            var response = await _mockService.Request200(default);
            return Ok(response);
        }
        [HttpGet("202")]
        public async Task<ActionResult<MockResponse>> Get202()
        {
            var response = await _mockService.Request202(default);
            return Ok(response);
        }
    }
}
