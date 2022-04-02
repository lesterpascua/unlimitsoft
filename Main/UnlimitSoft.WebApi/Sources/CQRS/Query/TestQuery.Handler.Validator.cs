﻿using FluentValidation;
using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Query;
using SoftUnlimit.CQRS.Query.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUnlimit.WebApi.Sources.CQRS.Query
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
