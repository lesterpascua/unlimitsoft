using System;
using System.Net;

namespace UnlimitSoft.Message;


/// <summary>
/// Standard eequest response
/// </summary>
/// <typeparam name="T"></typeparam>
public class Response<T> : IResponse
{
    /// <summary>
    /// Set internal and share visibility with mediator
    /// </summary>
    public bool _isNotMutable = false;

    /// <summary>
    /// Deserialization constructor
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Response() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="uiText"></param>
    /// <param name="traceId"></param>
    public Response(HttpStatusCode code, T body, string? uiText, string? traceId)
    {
        Code = code;
        Body = body;
        UIText = uiText;
        TraceIdentifier = traceId;
    }

    /// <summary>
    /// Body payload
    /// </summary>
    public T Body { get; set; }
    /// <inheritdoc />
    public HttpStatusCode Code { get; set; }
    /// <inheritdoc />
    public string? UIText { get; set; }
    /// <inheritdoc />
    public string? TraceIdentifier { get; set; }
    /// <inheritdoc />
    public bool IsSuccess => HttpStatusCode.OK <= Code && Code < HttpStatusCode.Ambiguous;

    /// <inheritdoc />
    public object? GetBody() => Body;
    /// <inheritdoc />
    public Type GetBodyType() => typeof(T);
    /// <inheritdoc />
    public bool IsInmutable() => _isNotMutable;
}