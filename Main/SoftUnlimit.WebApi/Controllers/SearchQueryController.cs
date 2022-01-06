using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Client;
using SoftUnlimit.Web.Model;
using SoftUnlimit.WebApi.Model;
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
