using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Message;
using UnlimitSoft.Web.AspNet;
using UnlimitSoft.Web.Model;
using UnlimitSoft.WebApi.Model;
using UnlimitSoft.WebApi.Sources.CQRS.Query;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Web;

namespace UnlimitSoft.WebApi.Controllers;


[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public sealed class SearchQueryController : ControllerBase
{
    private readonly IServiceProvider _provider;
    private readonly IQueryDispatcher _queryDispatcher;
    private readonly static ColumnName[] _order = new[] { new ColumnName { Name = nameof(Customer.Id), Asc = true } };


    /// <summary>
    /// 
    /// </summary>
    /// <param name="queryDispatcher"></param>
    public SearchQueryController(IServiceProvider provider, IQueryDispatcher queryDispatcher)
    {
        _provider = provider;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet]
    public async Task<ActionResult<SearchModel<Customer>>> Get([FromQuery] SearchCustomer vm)
    {
        var query = new SearchTestQuery(this.GetIdentity())
        {
            Filter = vm.Filter,
            Order = vm.Order ?? _order,
            Paging = vm.Paging
        };
        var result = await _queryDispatcher.DispatchAsync(_provider, query);

        return result.ToActionResult(this);
    }
}
