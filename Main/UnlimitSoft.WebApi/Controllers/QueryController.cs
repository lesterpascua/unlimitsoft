using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Message;
using UnlimitSoft.WebApi.Sources.CQRS.Query;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Web;
using System.Threading.Tasks;
using System;

namespace UnlimitSoft.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class QueryController : ControllerBase
{
    private readonly IServiceProvider _provider;
    private readonly IQueryDispatcher _queryDispatcher;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryDispatcher"></param>
    /// <param name="gen"></param>
    public QueryController(IServiceProvider provider, IQueryDispatcher queryDispatcher)
    {
        _provider = provider;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet]
    public async Task<ActionResult<Response<Customer[]>>> Get()
    {
        var query = new TestQuery(this.GetIdentity());
        var response = await _queryDispatcher.DispatchAsync(_provider, query);

        return this.ToActionResult(response);
    }
}
