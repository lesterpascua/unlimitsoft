using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Command;
using UnlimitSoft.Web.AspNet;
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
    public async Task<ActionResult<string>> Post(CancellationToken ct)
    {
        var command = new TestCommand(_gen.GenerateId(), this.GetIdentity());
        var result = await _dispatcher.DispatchAsync(command, ct);

        return result.ToActionResult(this);
    }
    /// <summary>
    /// Return exception and translate to bad response
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("response")]
    public async Task<ActionResult<string>> ErrResponse(CancellationToken ct)
    {
        var command = new ErrResponseCommand(_gen.GenerateId(), this.GetIdentity());
        var result = await _dispatcher.DispatchAsync(command, ct);

        return result.ToActionResult(this);
    }
}
