using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SoftUnlimit.CQRS.Query.Validation
{
    /// <summary>
    /// Base class for validate all query if data of command is correct. 
    /// </summary>
    /// <typeparam name="TQuery"></typeparam>
    [Obsolete("IQueryHandlerValidator")]
    public abstract class AbstractQueryValidator<TQuery> : AbstractValidator<TQuery>
        where TQuery : IQuery
    {
    }
}
