using UnlimitSoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace UnlimitSoft.Web.Client;


/// <summary>
/// 
/// </summary>
public class HttpException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="body"></param>
    public HttpException(HttpStatusCode code, string message, string body)
        : base(message)
    {
        Code = code;
        Body = body;
    }

    /// <summary>
    /// 
    /// </summary>
    public HttpStatusCode Code { get; }
    /// <summary>
    /// Body serialized as JSON used for response.
    /// </summary>
    public string Body { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        var message = Message.Replace(Environment.NewLine, $"{Environment.NewLine}   ");

        sb.AppendLine("Message: ").Append("   ").AppendLine(message);
        sb.AppendLine("StackTrace: ").AppendLine(StackTrace);

        if (!string.IsNullOrEmpty(Body))
            sb.AppendLine("Body: ").Append("   ").AppendLine(Body);
        return sb.ToString();
    }
}

/// <summary>
/// 
/// </summary>
public static class HttpExceptionExtensions
{
    /// <summary>
    /// Get standard response.
    /// </summary>
    /// <exception cref="Exception">
    /// If the respose don't have the specified scheme
    /// </exception>
    /// <param name="this"></param>
    /// <returns></returns>
    public static Response<Dictionary<string, string[]>>? GetResponse(this HttpException @this)
    {
        var body = @this.Body;
        return JsonUtility.Deserialize<Response<Dictionary<string, string[]>>>(body);
    }
}