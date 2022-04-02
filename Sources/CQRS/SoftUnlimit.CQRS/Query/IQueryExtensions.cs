using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Properties;
using System.Net;

namespace SoftUnlimit.CQRS.Query;

/// <summary>
/// 
/// </summary>
public static class IQueryExtensions
{
    private static readonly IQueryResponse _ok = new QueryResponse<object>(HttpStatusCode.OK, null, Resources.Response_OkResponse);


    /// <summary>
    /// Generate a success response using query data.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IQueryResponse QuickOkResponse(this IQuery _) => _ok;
    /// <summary>
    /// Generate a success response using query data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static IQueryResponse OkResponse<T>(this IQuery _, T body) => new QueryResponse<T>(HttpStatusCode.OK, body, Resources.Response_OkResponse);
    /// <summary>
    /// Generate a success response using query data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IQueryResponse OkResponse<T>(this IQuery<T> _, T body = default, string uiText = null) => new QueryResponse<T>(HttpStatusCode.OK, body, uiText ?? Resources.Response_OkResponse);


    /// <summary>
    /// Generate a bad response using query data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IQueryResponse BadResponse<T>(this IQuery _, T body, string uiText = null) => new QueryResponse<T>(HttpStatusCode.BadRequest, body, uiText ?? Resources.Response_BadResponse);
    /// <summary>
    /// Generate a error response using query data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IQueryResponse ErrorResponse<T>(this IQuery _, T body, string uiText = null) => new QueryResponse<T>(HttpStatusCode.InternalServerError, body, uiText ?? Resources.Response_ErrorResponse);
    /// <summary>
    /// Generate a not found response using query data.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IQueryResponse NotFoundResponse(this IQuery _, string uiText = null) => new QueryResponse<object>(HttpStatusCode.NotFound, null, uiText ?? Resources.Response_NotFoundResponse);
    /// <summary>
    /// Generate a response using query data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IQueryResponse Response<T>(this IQuery _, HttpStatusCode code, T body, string uiText) => new QueryResponse<T>(code, body, uiText);
}
