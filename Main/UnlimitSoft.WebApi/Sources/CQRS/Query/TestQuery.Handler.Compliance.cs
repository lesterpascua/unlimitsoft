using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// Handler
    /// </summary>
    public partial class TestQueryHandler : IQueryHandlerCompliance<TestQuery>
    {
        public ValueTask<IQueryResponse> ComplianceAsync(TestQuery query, CancellationToken ct = default)
        {
            return ValueTask.FromResult(query.QuickOkResponse());
        }
    }
}
