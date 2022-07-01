using SoftUnlimit.CQRS.Message;
using SoftUnlimit.CQRS.Properties;
using System;
using System.Collections.Generic;
using System.Net;

namespace SoftUnlimit.CQRS.Command;

/// <summary>
/// 
/// </summary>
public static class ICommandExtension
{
    private static readonly ICommandResponse _ok = new CommandResponse<object>(HttpStatusCode.OK, null, Resources.Response_OkResponse);
    private static readonly ICommandResponse _accepted = new CommandResponse<object>(HttpStatusCode.Accepted, null, Resources.Response_OkResponse);
    private static readonly ICommandResponse _notfound = new CommandResponse<object>(HttpStatusCode.NotFound, null, Resources.Response_NotFoundResponse);

    #region Accepted
    /// <summary>
    /// Use this to move over background step to avoid memory allocation.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static ICommandResponse QuickAcceptedResponse(this ICommand _) => _accepted;
    /// <summary>
    /// Generate a acepted response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static ICommandResponse AcceptedResponse(this ICommand _) => new CommandResponse<object>(HttpStatusCode.Accepted, null, Resources.Response_OkResponse);
    /// <summary>
    /// Generate a acepted response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static ICommandResponse AcceptedResponse<T>(this ICommand _, T body, string uiText = null) => new CommandResponse<object>(HttpStatusCode.Accepted, body, uiText ?? Resources.Response_OkResponse);
    #endregion

    #region Ok
    /// <summary>
    /// Use this to move over validation and compliance step to avoid memory allocation
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static ICommandResponse QuickOkResponse(this ICommand _) => _ok;
    /// <summary>
    /// Generate a success response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static ICommandResponse OkResponse(this ICommand _) => new CommandResponse<object>(HttpStatusCode.OK, null, Resources.Response_OkResponse);
    /// <summary>
    /// Generate a success response using command data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static ICommandResponse OkResponse<T>(this ICommand _, T body, string uiText = null) => new CommandResponse<T>(HttpStatusCode.OK, body, uiText ?? Resources.Response_OkResponse);
    #endregion

    #region BadResponse
    /// <summary>
    /// Generate a bad response using command data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static ICommandResponse BadResponse<T>(this ICommand _, T body, string uiText = null) => new CommandResponse<T>(HttpStatusCode.BadRequest, body, uiText ?? Resources.Response_BadResponse);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static ICommandResponse BadResponse(this ICommand _, Dictionary<string, string[]> body, string uiText = null) => new CommandResponse<Dictionary<string, string[]>>(HttpStatusCode.BadRequest, body, uiText ?? Resources.Response_BadResponse);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static ICommandResponse BadResponse(this ICommand @this, string key, string error, string uiText = null) => @this.BadResponse(new Dictionary<string, string[]> { [key] = new string[] { error } }, uiText);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static ICommandResponse BadResponse(this ICommand @this, string key, int error, string uiText = null) => @this.BadResponse(key, error.ToString(), uiText);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="this"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static ICommandResponse BadResponse<TError>(this ICommand @this, string key, TError error, string uiText = null) where TError : Enum => @this.BadResponse(key, error.ToString("D"), uiText);
    #endregion

    #region ErrorResponse
    /// <summary>
    /// Generate a error response using command data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static ICommandResponse ErrorResponse<T>(this ICommand _, T body, string uiText = null) => new CommandResponse<T>(HttpStatusCode.InternalServerError, body, uiText ?? Resources.Response_ErrorResponse);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <returns></returns>
    public static ICommandResponse ErrorResponse(this ICommand _, Dictionary<string, string[]> body, string uiText = null) => new CommandResponse<Dictionary<string, string[]>>(HttpStatusCode.InternalServerError, body, uiText ?? Resources.Response_ErrorResponse);
    #endregion

    #region NotFound
    /// <summary>
    /// Use this to move over validation and compliance step to avoid memory allocation
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    public static ICommandResponse QuickNotFoundResponse(this ICommand _) => _notfound;
    /// <summary>
    /// Generate a not found response using command data.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static ICommandResponse NotFoundResponse(this ICommand _, string uiText = null) => new CommandResponse<object>(HttpStatusCode.NotFound, null, uiText ?? Resources.Response_NotFoundResponse);
    #endregion

    /// <summary>
    /// Generate a response using command data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="_"></param>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="uiText">Global information about message</param>
    /// <returns></returns>
    public static ICommandResponse Response<T>(this ICommand _, HttpStatusCode code, T body, string uiText) => new CommandResponse<T>(code, body, uiText);
}