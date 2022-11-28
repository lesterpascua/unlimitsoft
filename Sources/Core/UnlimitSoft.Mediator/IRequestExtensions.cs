using System;
using System.Collections.Generic;
using System.Net;
using UnlimitSoft.Mediator.Properties;
using UnlimitSoft.Message;

namespace UnlimitSoft.Mediator;


/// <summary>
/// 
/// </summary>
public static class IRequestExtensions
{
    private static readonly IResponse _200 = new Response<object?>(HttpStatusCode.OK, null, Resources.Response_OkResponse, null) { _isNotMutable = true };
    private static readonly IResponse _400 = new Response<object?>(HttpStatusCode.BadRequest, null, Resources.Response_BadResponse, null) { _isNotMutable = true };
    private static readonly IResponse _202 = new Response<object?>(HttpStatusCode.Accepted, null, Resources.Response_AcceptResponse, null) { _isNotMutable = true };
    private static readonly IResponse _404 = new Response<object?>(HttpStatusCode.NotFound, null, Resources.Response_NotFoundResponse, null) { _isNotMutable = true };
    private static readonly IResponse _500 = new Response<object?>(HttpStatusCode.InternalServerError, null, Resources.Response_ErrorResponse, null) { _isNotMutable = true };

    #region 200
    /// <summary>
    /// Use this to move over validation and compliance step to avoid memory allocation
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IResponse OkResponse(this IRequest _) => _200;
    /// <summary>
    /// Generate a success response using command data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IResponse OkResponse<T>(this IRequest _, T body, string? uiText = null) => new Response<T>(HttpStatusCode.OK, body, uiText ?? Resources.Response_OkResponse, null);
    #endregion

    #region 202
    /// <summary>
    /// Generate a acepted response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IResponse AcceptedResponse(this IResponse _) => _202;
    /// <summary>
    /// Generate a acepted response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse AcceptedResponse<T>(this IResponse _, T body, string? uiText = null) => new Response<T>(HttpStatusCode.Accepted, body, uiText ?? Resources.Response_OkResponse, null);
    #endregion

    #region 400
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest _) => _400;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse BadResponse<T>(this IRequest _, T body, string? uiText) => new Response<T>(HttpStatusCode.BadRequest, body, uiText ?? Resources.Response_BadResponse, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest _, IDictionary<string, string[]> body, string? uiText = null) => new Response<IDictionary<string, string[]>>(HttpStatusCode.BadRequest, body, uiText ?? Resources.Response_BadResponse, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest _, string key, string error, string? uiText = null) => new Response<IDictionary<string, string[]>>(HttpStatusCode.BadRequest, new Dictionary<string, string[]> { [key] = new string[] { error } }, uiText ?? Resources.Response_BadResponse, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest @this, string key, int error, string? uiText = null) => @this.BadResponse(key, error.ToString(), uiText);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse BadResponse<TError>(this IRequest @this, string key, TError error, string? uiText = null) where TError : Enum => @this.BadResponse(key, error.ToString("D"), uiText);
    #endregion

    #region 404
    /// <summary>
    /// Use this to move over validation and compliance step to avoid memory allocation
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IResponse QuickNotFoundResponse(this IRequest _) => _404;
    /// <summary>
    /// Generate a not found response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IResponse NotFoundResponse<T>(this IRequest _, T body, string? uiText = null) => new Response<T>(HttpStatusCode.NotFound, body, uiText ?? Resources.Response_NotFoundResponse, null);
    #endregion

    #region 500
    /// <summary>
    /// Generate a error response using command data
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IResponse ErrorResponse(this IRequest _) => _500;
    /// <summary>
    /// Generate a error response using command data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static IResponse ErrorResponse<T>(this IRequest _, T body, string? uiText = null) => new Response<T>(HttpStatusCode.InternalServerError, body, uiText ?? Resources.Response_ErrorResponse, null);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static IResponse ErrorResponse(this IRequest _, Dictionary<string, string[]> body, string? uiText = null) => new Response<Dictionary<string, string[]>>(HttpStatusCode.InternalServerError, body, uiText ?? Resources.Response_ErrorResponse, null);
    #endregion

    /// <summary>
    /// Indicate if the response is a mutable object.
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static bool IsMutable(this IResponse @this) =>
        ReferenceEquals(@this, _200) || ReferenceEquals(@this, _202) ||
        ReferenceEquals(@this, _400) || ReferenceEquals(@this, _404) ||
        ReferenceEquals(@this, _500);
}