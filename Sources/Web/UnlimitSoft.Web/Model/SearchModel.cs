using System.Collections.Generic;

namespace UnlimitSoft.Web.Model;


/// <summary>
/// Result for all search operations
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <param name="Total">Total amount of element in the query</param>
/// <param name="Data">Paginated data</param>
public record SearchModel<TModel>(long Total, IEnumerable<TModel> Data);
/// <summary>
/// Result for all search operations
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <param name="Total">Total amount of element in the query</param>
/// <param name="Page">Page resulting of the search</param>
/// <param name="PageSize">Page size resulting of the search</param>
/// <param name="Data">Paginated data</param>
public record SearchXModel<TModel>(long Total, int Page, int PageSize, IEnumerable<TModel> Data);