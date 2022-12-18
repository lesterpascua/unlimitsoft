using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Mediator;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query;

/// <summary>
/// Handler
/// </summary>
public partial class TestQueryHandler : IQueryHandlerCompliance<TestQuery>
{
    public ValueTask<IResponse> ComplianceV2Async(TestQuery query, CancellationToken ct = default)
    {
        return ValueTask.FromResult(query.OkResponse());
    }
}
