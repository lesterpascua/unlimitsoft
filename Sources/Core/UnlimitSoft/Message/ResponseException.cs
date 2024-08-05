using System;
using System.Collections.Generic;
using System.Net;

namespace UnlimitSoft.Message;


/// <summary>
/// Indicate special exception in the response.
/// </summary>
public class ResponseException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="code"></param>
    /// <param name="innerException"></param>
    public ResponseException(string message, HttpStatusCode code = HttpStatusCode.InternalServerError, Exception? innerException = null)
        : this(message, new Dictionary<string, string[]> { { string.Empty, new string[] { message } } }, code, innerException)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <param name="code"></param>
    /// <param name="innerException"></param>
    public ResponseException(IDictionary<string, string[]> body, HttpStatusCode code = HttpStatusCode.InternalServerError, Exception? innerException = null)
        : this("Operation has error, see body for more details.", body, code, innerException)
    {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="body"></param>
    /// <param name="code"></param>
    /// <param name="innerException"></param>
    public ResponseException(string message, IDictionary<string, string[]> body, HttpStatusCode code = HttpStatusCode.InternalServerError, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Body = body;
    }

    /// <summary>
    /// 
    /// </summary>
    public HttpStatusCode Code { get; init; }
    /// <summary>
    /// Exception body.
    /// </summary>
    public IDictionary<string, string[]> Body { get; init; }
}