using Microsoft.Extensions.Logging;
using OIU.Libs.Logger;
using System.Net;
using UnlimitSoft.Json;
using UnlimitSoft.Message;
using UnlimitSoft.Web.Client;

namespace OIU.Libs.Web;


/// <summary>
/// Helper class for http request over onejn api
/// </summary>
public static class Http
{
    /// <summary>
    /// Build standard one jn error body
    /// </summary>
    /// <param name="err"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static Dictionary<string, string[]> GetBody(string err, string? field = null)
    {
        field ??= string.Empty;
        return new() { [field] = [err] };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="errs"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    public static Dictionary<string, string[]> GetBody(string[] errs, string? field = null)
    {
        field ??= string.Empty;
        return new() { [field] = errs };
    }

    /// <summary>
    /// Call onejn service and catch any posible exception to convert into result object to use result patter instead raice exception
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serializer"></param>
    /// <param name="action"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static async Task<Result<T>> GetResultAsync<T>(IJsonSerializer serializer, Func<Task<T>> action, ILogger? logger)
    {
        try
        {
            var result = await action();
            return Result.Ok(result);
        }
        catch (HttpException ex)
        {
            var body = ex.GetBody(serializer) ?? GetBody(WKE.Error);
            return Result.Err<T>(new ErrorResponse(ex.Code, body));
        }
        catch (AggregateException e) when (e.InnerException is HttpException ex)
        {
            var body = ex.GetBody(serializer) ?? GetBody(WKE.Error);
            return Result.Err<T>(new ErrorResponse(ex.Code, body));
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unknow exception in http request");

            var body = GetBody(WKE.Error);
            return Result.Err<T>(new ErrorResponse(HttpStatusCode.InternalServerError, body));
        }
    }

    /// <summary>
    /// Convert <see cref="HttpException"/> into <see cref="ResponseException"/> 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="ex"></param>
    /// <param name="code"></param>
    /// <exception cref="ResponseException"></exception>
    public static Result<T> GetResult<T>(IJsonSerializer serializer, HttpException ex, HttpStatusCode code = HttpStatusCode.BadRequest)
    {
        return GetResult<T>(serializer, ex, WKE.Error, code);
    }
    /// <summary>
    /// Convert <see cref="HttpException"/> into <see cref="ResponseException"/> 
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="ex"></param>
    /// <param name="err"></param>
    /// <param name="code"></param>
    /// <exception cref="ResponseException"></exception>
    public static Result<T> GetResult<T>(IJsonSerializer serializer, HttpException ex, string err, HttpStatusCode code = HttpStatusCode.BadRequest)
    {
        var body = ex.GetBody(serializer);
        if (body is null)
            return Result.Err<T>(err, code: code);

        return Result.Err<T>(new ErrorResponse(code, body));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <param name="code"></param>
    /// <exception cref="ResponseException"></exception>
    public static Result<T> GetResult<T>(IDictionary<string, string[]> body, HttpStatusCode code = HttpStatusCode.BadRequest) => Result.Err<T>(new ErrorResponse(code, body));
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="errs"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="ResponseException"></exception>
    public static Result<T> GetResult<T>(string[] errs, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest) => Result.Err<T>(new ErrorResponse(code, GetBody(errs, field)));
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="err"></param>
    /// <param name="field"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    public static Result<T> GetResult<T>(string err, string? field = null, HttpStatusCode code = HttpStatusCode.BadRequest) => Result.Err<T>(new ErrorResponse(code, GetBody(err, field)));
}
