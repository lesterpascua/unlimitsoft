using UnlimitSoft.Mediator;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Interface to define a Query
/// </summary>
public interface IQuery : IRequest
{
    /// <summary>
    /// Return metadata props associate with the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T? GetProps<T>() where T : QueryProps;
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IQuery<out TResponse> : IQuery, IRequest<TResponse>
{
}
