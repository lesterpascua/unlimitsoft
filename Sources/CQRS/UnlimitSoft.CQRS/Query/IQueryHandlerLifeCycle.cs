using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Indicate the command handler allow LifeCycle him self
/// </summary>
public interface IQueryHandlerLifeCycle<TQuery> : IQueryHandler, IRequestHandlerLifeCycle<TQuery>
    where TQuery : IQuery
{
}
