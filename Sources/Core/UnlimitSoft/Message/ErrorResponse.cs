using System;
using System.Collections.Generic;
using System.Net;

namespace UnlimitSoft.Message;


/// <summary>
/// Standard proposal to represent a response with error
/// </summary>
public sealed class ErrorResponse : Response<IDictionary<string, string[]>?>
{
    /// <summary>
    /// 
    /// </summary>
    public ErrorResponse() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    public ErrorResponse(HttpStatusCode code, IDictionary<string, string[]>? body) : base(code, body)
    {
    }

    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IDictionary<string, string[]> GetError(string key, int error) => GetError(key, [error.ToString()]);
    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ErrorResponse GetError(HttpStatusCode code, string key, int error) => GetError(code, key, [error.ToString()]);

    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ErrorResponse GetError(HttpStatusCode code, string key, int[] error)
    {
        var err = new string[error.Length];
        for (var i = 0; i < error.Length; i++)
            err[i] = error[i].ToString();
        return GetError(code, key, err);
    }

    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IDictionary<string, string[]> GetError(string key, string error) => GetError(key, [error]);
    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ErrorResponse GetError(HttpStatusCode code, string key, string error) => GetError(code, key, [error]);

    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IDictionary<string, string[]> GetError(string key, string[] error)
    {
        return new Dictionary<string, string[]> { [key] = error };
    }
    /// <summary>
    /// Build an error response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ErrorResponse GetError(HttpStatusCode code, string key, string[] error)
    {
        var body = new Dictionary<string, string[]> { [key] = error };
        return new ErrorResponse(code, body);
    }

    /// <summary>
    /// Build an error response
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IDictionary<string, string[]> GetError<TError>(string key, TError error) where TError : Enum => GetError(key, [error.ToString("D")]);
    /// <summary>
    /// Build an error response
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ErrorResponse GetError<TError>(HttpStatusCode code, string key, TError error) where TError : Enum => GetError(code, key, [error.ToString("D")]);

    /// <summary>
    /// Build an error response
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static IDictionary<string, string[]> GetError<TError>(string key, TError[] error) where TError : Enum
    {
        var err = new string[error.Length];
        for (var i = 0; i < error.Length; i++)
            err[i] = error[i].ToString("D");
        return GetError(key, err);
    }
    /// <summary>
    /// Build an error response
    /// </summary>
    /// <typeparam name="TError"></typeparam>
    /// <param name="code"></param>
    /// <param name="key"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static ErrorResponse GetError<TError>(HttpStatusCode code, string key, TError[] error) where TError : Enum
    {
        var err = new string[error.Length];
        for (var i = 0; i < error.Length; i++)
            err[i] = error[i].ToString("D");
        return GetError(code, key, err);
    }
}