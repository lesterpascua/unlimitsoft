using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// 
/// </summary>
public interface IQueryHandler : IRequestHandler
{
}
/// <summary>
/// Query handle
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TQuery"></typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IQueryHandler, IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
