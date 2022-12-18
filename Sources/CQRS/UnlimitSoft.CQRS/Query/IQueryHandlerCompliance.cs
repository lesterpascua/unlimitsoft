using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Indicate the command handler allow compliance him self
/// </summary>
public interface IQueryHandlerCompliance<TQuery> : IQueryHandler, IRequestHandlerCompliance<TQuery>
    where TQuery: IQuery
{
}
