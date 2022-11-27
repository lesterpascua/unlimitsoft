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
    /// <param name="uiText"></param>
    /// <param name="traceId"></param>
    public ErrorResponse(HttpStatusCode code, IDictionary<string, string[]>? body, string? uiText, string? traceId) : base(code, body, uiText, traceId)
    {
    }
}