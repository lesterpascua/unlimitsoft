using FluentValidation;
using SoftUnlimit.CQRS.Query.Validation;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// Indicate the command handler allow validate him self
    /// </summary>
    public interface IQueryHandlerValidator<TQuery> : IQueryHandler
        where TQuery : IQuery
    {
        /// <summary>
        /// Build fluen validation rules.
        /// </summary>
        /// <param name="validator"></param>
        IValidator BuildValidator(QueryValidator<TQuery> validator);
    }
}
