using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftUnlimit.CQRS.Command;
using SoftUnlimit.WebApi.Sources.CQRS.Command;
using SoftUnlimit.WebApi.Sources.Security.Cryptography;
using SoftUnlimit.WebApi.Sources.Web;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class CommandController : ControllerBase
    {
        private readonly IMyIdGenerator _gen;
        private readonly ICommandDispatcher _dispatcher;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="gen"></param>
        public CommandController(IMyIdGenerator gen, ICommandDispatcher dispatcher)
        {
            _gen = gen;
            _dispatcher = dispatcher;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(CancellationToken ct)
        {
            var command = new TestCommand(_gen.GenerateId(), this.GetIdentity());
            var response = await _dispatcher.DispatchAsync(command, ct);

            return this.ToActionResult(response);
        }
    }
}
