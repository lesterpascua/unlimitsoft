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
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="body"></param>
    /// <param name="traceId"></param>
    [Obsolete("Response don't need trace identifier keep only for backward compatibility")]
    public ErrorResponse(HttpStatusCode code, IDictionary<string, string[]>? body, string? traceId) : base(code, body, traceId)
    {
    }
}