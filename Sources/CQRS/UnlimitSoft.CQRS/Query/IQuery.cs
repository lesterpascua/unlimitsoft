using UnlimitSoft.Message;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Interface to define a Query
/// </summary>
public interface IQuery : IRequest
{
    /// <summary>
    /// Return metadata props associate with the query.
    /// </summary>
    /// <returns></returns>
    QueryProps GetProps();
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IQuery<out TResponse> : IQuery, IRequest<TResponse>
{
}
