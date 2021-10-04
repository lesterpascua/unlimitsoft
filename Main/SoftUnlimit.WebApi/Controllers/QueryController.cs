using Microsoft.AspNetCore.Mvc;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.Security.Cryptography;
using SoftUnlimit.Web.Client;
using SoftUnlimit.WebApi.Sources.CQRS.Query;
using SoftUnlimit.WebApi.Sources.Data.Model;
using SoftUnlimit.WebApi.Sources.Security.Cryptography;
using SoftUnlimit.WebApi.Sources.Web;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly IMyIdGenerator _gen;
        private readonly IQueryDispatcher _queryDispatcher;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryDispatcher"></param>
        /// <param name="gen"></param>
        public QueryController(IMyIdGenerator gen, IQueryDispatcher queryDispatcher)
        {
            _gen = gen;
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
