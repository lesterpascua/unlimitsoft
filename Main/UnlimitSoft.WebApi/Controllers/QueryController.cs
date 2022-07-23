using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Web.Client;
using UnlimitSoft.WebApi.Sources.CQRS.Query;
using UnlimitSoft.WebApi.Sources.Data.Model;
using UnlimitSoft.WebApi.Sources.Web;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Controllers
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
            var (response, _) = await query.ExecuteAsync(_queryDispatcher);

            return this.ToActionResult(response);
        }
    }
}
