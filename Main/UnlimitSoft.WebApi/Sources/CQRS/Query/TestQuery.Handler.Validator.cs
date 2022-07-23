using FluentValidation;
using UnlimitSoft.CQRS.Message;
using UnlimitSoft.CQRS.Query;
using UnlimitSoft.CQRS.Query.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// Handler
    /// </summary>
    public partial class TestQueryHandler : IQueryHandlerValidator<TestQuery>
    {
        public ValueTask<IQueryResponse> ValidatorAsync(TestQuery query, QueryValidator<TestQuery> validator, CancellationToken ct = default)
        {
            validator.RuleFor(p => p.Name).Must(name =>
            {
                return true;
            });
            return ValueTask.FromResult(query.QuickOkResponse());
        }
    }
}
