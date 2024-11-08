using UnlimitSoft.Web.Model;
using System.Collections.Generic;

namespace UnlimitSoft.CQRS.Query;


/// <summary>
/// Interface for al search query to stablish uniform patter
/// </summary>
public interface IQuerySearch
{
    /// <summary>
    /// 
    /// </summary>
    Paging Paging { get; init; }
    /// <summary>
    /// 
    /// </summary>
    IReadOnlyList<ColumnName> Order { get; init; }
}
/// <summary>
/// 
/// </summary>
/// <typeparam name="TFilter"></typeparam>
public interface IQuerySearch<TFilter> : IQuerySearch
{
    /// <summary>
    /// 
    /// </summary>
    TFilter? Filter { get; init; }
}