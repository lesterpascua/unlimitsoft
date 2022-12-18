using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Indicate the command handler allow validate him self
/// </summary>
public interface IQueryHandlerValidator<TQuery> : IQueryHandler, IRequestHandlerValidator<TQuery>
    where TQuery : IQuery
{
}
