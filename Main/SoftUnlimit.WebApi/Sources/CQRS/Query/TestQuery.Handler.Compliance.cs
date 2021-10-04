using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// Handler
    /// </summary>
    public partial class TestQueryHandler : IQueryHandlerCompliance<TestQuery>
    {
        public Task<IQueryResponse> HandleComplianceAsync(TestQuery query, CancellationToken ct = default)
        {
            return Task.FromResult(query.OkResponse());
        }
    }
}
