using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.Mediator;
using UnlimitSoft.Mediator.Validation;
using UnlimitSoft.Message;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query;


/// <summary>
/// Handler
/// </summary>
public partial class TestQueryHandler : IQueryHandlerValidator<TestQuery>
{
    public ValueTask<IResponse> ValidatorV2Async(TestQuery query, RequestValidator<TestQuery> validator, CancellationToken ct = default)
    {
        validator.RuleFor(p => p.Name)
            .Must(name =>
            {
                return true;
            });
        return ValueTask.FromResult(query.OkResponse());
    }
}
