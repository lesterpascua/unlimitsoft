﻿using Microsoft.AspNetCore.Authorization;
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
    public class AsyncCommandController : ControllerBase
    {
        private readonly IMyIdGenerator _gen;
        private readonly ICommandBus _bus;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="bus"></param>
        public AsyncCommandController(IMyIdGenerator gen, ICommandBus bus)
        {
            _gen = gen;
            _bus = bus;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post(CancellationToken ct)
        {
            var command = new AsyncTestCommand(_gen.GenerateId(), this.GetIdentity());
            var response = await _bus.SendAsync(command, ct);

            return Accepted(response);
        }
    }
}