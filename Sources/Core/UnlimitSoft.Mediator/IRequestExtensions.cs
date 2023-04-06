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
    private static readonly IResponse _200 = new Response<object?>(HttpStatusCode.OK, null) { _isNotMutable = true };
    private static readonly IResponse _400 = new Response<object?>(HttpStatusCode.BadRequest, null) { _isNotMutable = true };
    private static readonly IResponse _202 = new Response<object?>(HttpStatusCode.Accepted, null) { _isNotMutable = true };
    private static readonly IResponse _404 = new Response<object?>(HttpStatusCode.NotFound, null) { _isNotMutable = true };
    private static readonly IResponse _500 = new Response<object?>(HttpStatusCode.InternalServerError, null) { _isNotMutable = true };

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
    /// <returns></returns>
    public static IResponse OkResponse<T>(this IRequest _, T body) => new Response<T>(HttpStatusCode.OK, body);
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
    /// <returns></returns>
    public static IResponse AcceptedResponse<T>(this IResponse _, T body) => new Response<T>(HttpStatusCode.Accepted, body);
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
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest _, IDictionary<string, string[]> body) => new ErrorResponse(HttpStatusCode.BadRequest, body);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest _, string key, string error) => ResponseUtil.GetError(HttpStatusCode.BadRequest, key, error);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IResponse BadResponse(this IRequest _, string key, int error) => ResponseUtil.GetError(HttpStatusCode.BadRequest, key, error);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IResponse BadResponse<TError>(this IRequest _, string key, TError error) where TError : Enum => ResponseUtil.GetError(HttpStatusCode.BadRequest, key, error);
    #endregion

    #region 404
    /// <summary>
    /// Use this to move over validation and compliance step to avoid memory allocation
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static IResponse NotFoundResponse(this IRequest _) => _404;
    /// <summary>
    /// Generate a not found response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static IResponse NotFoundResponse(this IRequest _, IDictionary<string, string[]> body) => new ErrorResponse(HttpStatusCode.NotFound, body);
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
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public static IResponse ErrorResponse(this IRequest _, IDictionary<string, string[]> body) => new ErrorResponse(HttpStatusCode.InternalServerError, body);
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