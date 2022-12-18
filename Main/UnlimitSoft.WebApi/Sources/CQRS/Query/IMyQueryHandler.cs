using UnlimitSoft.CQRS.Query;

namespace UnlimitSoft.WebApi.Sources.CQRS.Query;


/// <summary>
/// 
/// </summary>
/// <typeparam name="TResponse"></typeparam>
/// <typeparam name="TQuery"></typeparam>
public interface IMyQueryHandler<in TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
    where TQuery : MyQuery<TResponse>
{
}
