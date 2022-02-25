using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Properties;

namespace SoftUnlimit.CQRS.Query
{
    /// <summary>
    /// 
    /// </summary>
    public static class IQueryExtensions
    {
        /// <summary>
        /// Generate a success response using query data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipqueryInfo"></param>
        /// <returns></returns>
        public static IQueryResponse OkResponse<T>(this IQuery query, T body, string uiText = null, bool skipqueryInfo = true) => new QueryResponse<T>(skipqueryInfo ? new SealedQueryAsync<T>(query.GetProps<QueryProps>()) : query, 200, body, uiText ?? Resources.Response_OkResponse);
        /// <summary>
        /// Generate a success response using query data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipqueryInfo"></param>
        /// <returns></returns>
        public static IQueryResponse OkResponse<T>(this IQuery<T> query, T body = default, string uiText = null, bool skipqueryInfo = true) => new QueryResponse<T>(skipqueryInfo ? new SealedQueryAsync<T>(query.GetProps<QueryProps>()) : query, 200, body, uiText ?? Resources.Response_OkResponse);


        /// <summary>
        /// Generate a bad response using query data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipqueryInfo"></param>
        /// <returns></returns>
        public static IQueryResponse BadResponse<T>(this IQuery query, T body, string uiText = null, bool skipqueryInfo = true) => new QueryResponse<T>(skipqueryInfo ? new SealedQueryAsync<T>(query.GetProps<QueryProps>()) : query, 400, body, uiText ?? Resources.Response_BadResponse);
        /// <summary>
        /// Generate a error response using query data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipqueryInfo"></param>
        /// <returns></returns>
        public static IQueryResponse ErrorResponse<T>(this IQuery query, T body, string uiText = null, bool skipqueryInfo = true) => new QueryResponse<T>(skipqueryInfo ? new SealedQueryAsync<T>(query.GetProps<QueryProps>()) : query, 500, body, uiText ?? Resources.Response_ErrorResponse);
        /// <summary>
        /// Generate a not found response using query data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipqueryInfo"></param>
        /// <returns></returns>
        public static IQueryResponse NotFoundResponse<T>(this IQuery query, T body, string uiText = null, bool skipqueryInfo = true) => new QueryResponse<T>(skipqueryInfo ? new SealedQueryAsync<T>(query.GetProps<QueryProps>()) : query, 404, body, uiText ?? Resources.Response_NotFoundResponse);
        /// <summary>
        /// Generate a response using query data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="code"></param>
        /// <param name="body"></param>
        /// <param name="uiText">Global information about message</param>
        /// <param name="skipqueryInfo"></param>
        /// <returns></returns>
        public static IQueryResponse Response<T>(this IQuery query, int code, T body, string uiText, bool skipqueryInfo = true) => new QueryResponse<T>(skipqueryInfo ? new SealedQueryAsync<T>(query.GetProps<QueryProps>()) : query, code, body, uiText);
    }
}
