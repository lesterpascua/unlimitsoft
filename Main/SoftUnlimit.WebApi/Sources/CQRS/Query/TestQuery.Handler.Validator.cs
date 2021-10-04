using FluentValidation;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Validation;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
{
    /// <summary>
    /// Handler
    /// </summary>
    public partial class TestQueryHandler : IQueryHandlerValidator<TestQuery>
    {
        public IValidator BuildValidator(QueryValidator<TestQuery> validator)
        {
            validator.RuleFor(p => p.Name).Must(name =>
            {
                return true;
            });
            return validator;
        }
    }
}
