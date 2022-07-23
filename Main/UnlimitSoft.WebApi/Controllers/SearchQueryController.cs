using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.Client;
using UnlimitSoft.Web.Model;
using UnlimitSoft.WebApi.Model;
using UnlimitSoft.WebApi.Sources.CQRS.Query;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Security.Cryptography;
using UnlimitSoft.WebApi.Sources.Web;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class SearchQueryController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryDispatcher"></param>
        public SearchQueryController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult<Response<SearchModel<Customer>>>> Get([FromQuery] SearchCustomer vm)
        {
            var query = new SearchTestQuery(this.GetIdentity())
            {
                Filter = vm.Filter,
                Order = vm.Order,
                Paging = vm.Paging
            };
            var (response, _) = await query.ExecuteAsync(_queryDispatcher);

            return this.ToActionResult(response);
        }
    }
}
