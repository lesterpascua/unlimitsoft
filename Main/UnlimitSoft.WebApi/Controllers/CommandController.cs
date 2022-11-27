﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.WebApi.Sources.CQRS.Command;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using UnlimitSoft.WebApi.Sources.Web;

namespace UnlimitSoft.WebApi.Controllers;


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
        var result = await _dispatcher.DispatchAsync(command, ct);

        return this.ToActionResult(result);
    }
}
