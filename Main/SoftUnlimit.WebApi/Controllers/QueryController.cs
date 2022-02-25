using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Web.Client;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using SoftUnlimit.WebApi.Sources.Data.Model;
using SoftUnlimit.WebApi.Sources.Web;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous]
    public class QueryController : ControllerBase
    {
        private readonly IQueryDispatcher _queryDispatcher;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryDispatcher"></param>
        /// <param name="gen"></param>
        public QueryController(IQueryDispatcher queryDispatcher)
        {
            _queryDispatcher = queryDispatcher;
        }

        [HttpGet]
        public async Task<ActionResult<Response<Customer[]>>> Get()
        {
            var query = new TestQuery(this.GetIdentity());
            var response = await query.ExecuteAsync(_queryDispatcher);

            return this.ToActionResult(response);
        }
    }
}
