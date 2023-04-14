using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.AspNet;
using UnlimitSoft.Web.Model;
using UnlimitSoft.WebApi.Sources.CQRS.Query;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Web;

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
    public async Task<ActionResult<SearchModel<Customer>>> Get()
    {
        var query = new TestQuery(this.GetIdentity());
        var result = await _queryDispatcher.DispatchAsync(_provider, query);

        return result.ToActionResult(this);
    }
}
